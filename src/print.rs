use printers::common::base::job::PrinterJobOptions;
use serde::{Deserialize, Serialize};
use std::path::{Path, PathBuf};
use tokio::sync::mpsc;
use tracing::{error, info, warn};

const PREFETCH_BUFFER: usize = 5;

#[derive(Debug, Clone, Serialize)]
pub struct PrinterInfo {
    pub name: String,
    pub is_default: bool,
}

#[derive(Debug, Deserialize)]
pub struct PrintRequest {
    pub printer: String,
    pub files: Vec<FileEntry>,
}

#[derive(Debug, Deserialize)]
pub struct FileEntry {
    pub url: String,
    pub filename: Option<String>,
}

#[derive(Debug, Serialize)]
pub struct PrintResult {
    pub total: usize,
    pub successful: usize,
    pub failed: usize,
    pub errors: Vec<String>,
}

struct DownloadedFile {
    index: usize,
    path: PathBuf,
    _temp_dir: tempfile::TempDir,
}

pub async fn list_printers() -> Result<Vec<PrinterInfo>, String> {
    let printers = tokio::task::spawn_blocking(|| printers::get_printers())
        .await
        .map_err(|e| format!("Failed to enumerate printers: {}", e))?;

    Ok(printers
        .into_iter()
        .map(|p| PrinterInfo {
            name: p.name.clone(),
            is_default: p.is_default,
        })
        .collect())
}

pub async fn process_print_request(request: PrintRequest) -> PrintResult {
    let total = request.files.len();

    info!(
        printer = %request.printer,
        file_count = total,
        "Processing print request"
    );

    let (tx, rx) = mpsc::channel::<Result<DownloadedFile, (usize, String)>>(PREFETCH_BUFFER);

    let files = request.files;
    let printer_name = request.printer;

    let download_handle = tokio::spawn(async move {
        let client = reqwest::Client::builder()
            .timeout(std::time::Duration::from_secs(300))
            .build()
            .unwrap_or_default();

        for (i, file_entry) in files.iter().enumerate() {
            info!(
                file = i + 1,
                total,
                url = %file_entry.url,
                "Downloading file"
            );

            match download_to_temp(&client, file_entry, i).await {
                Ok(downloaded) => {
                    info!(
                        file = i + 1,
                        total,
                        path = %downloaded.path.display(),
                        "File downloaded, queued for printing"
                    );
                    if tx.send(Ok(downloaded)).await.is_err() {
                        warn!("Print consumer dropped, stopping downloads");
                        break;
                    }
                }
                Err(e) => {
                    if tx.send(Err((i, e))).await.is_err() {
                        break;
                    }
                }
            }
        }
    });

    let print_result = print_from_channel(rx, &printer_name, total).await;
    download_handle.await.ok();

    print_result
}

async fn download_to_temp(
    client: &reqwest::Client,
    file_entry: &FileEntry,
    index: usize,
) -> Result<DownloadedFile, String> {
    use futures_util::StreamExt;
    use tokio::io::AsyncWriteExt;

    let extension = file_entry
        .filename
        .as_deref()
        .or_else(|| file_entry.url.rsplit('/').next())
        .and_then(|name| {
            let name = name.split('?').next().unwrap_or(name);
            Path::new(name).extension().and_then(|ext| ext.to_str())
        })
        .unwrap_or("pdf");

    let temp_dir =
        tempfile::tempdir().map_err(|e| format!("Failed to create temp directory: {}", e))?;
    let temp_path = temp_dir
        .path()
        .join(format!("print_{:04}.{}", index, extension));

    let response = client
        .get(&file_entry.url)
        .send()
        .await
        .map_err(|e| format!("Download request failed: {}", e))?;

    if !response.status().is_success() {
        return Err(format!(
            "Download failed with status: {}",
            response.status()
        ));
    }

    let mut file = tokio::fs::File::create(&temp_path)
        .await
        .map_err(|e| format!("Failed to create temp file: {}", e))?;

    let mut stream = response.bytes_stream();
    let mut bytes_written: u64 = 0;

    while let Some(chunk) = stream.next().await {
        let chunk = chunk.map_err(|e| format!("Download stream error: {}", e))?;
        file.write_all(&chunk)
            .await
            .map_err(|e| format!("Failed to write to temp file: {}", e))?;
        bytes_written += chunk.len() as u64;
    }

    file.flush()
        .await
        .map_err(|e| format!("Failed to flush temp file: {}", e))?;
    drop(file);

    info!(bytes = bytes_written, "File downloaded to temp");

    Ok(DownloadedFile {
        index,
        path: temp_path,
        _temp_dir: temp_dir,
    })
}

async fn print_from_channel(
    mut rx: mpsc::Receiver<Result<DownloadedFile, (usize, String)>>,
    printer_name: &str,
    total: usize,
) -> PrintResult {
    let mut successful = 0;
    let mut errors = Vec::new();

    while let Some(item) = rx.recv().await {
        match item {
            Ok(downloaded) => {
                let file_num = downloaded.index + 1;
                info!(
                    file = file_num,
                    total,
                    path = %downloaded.path.display(),
                    "Sending file to printer"
                );

                match send_to_printer(&downloaded.path, printer_name).await {
                    Ok(()) => {
                        successful += 1;
                        info!(file = file_num, total, "File printed successfully");
                    }
                    Err(e) => {
                        let msg = format!("File {}: {}", file_num, e);
                        error!("{}", msg);
                        errors.push(msg);
                    }
                }
            }
            Err((index, e)) => {
                let msg = format!("File {}: {}", index + 1, e);
                error!("{}", msg);
                errors.push(msg);
            }
        }
    }

    let failed = total - successful;
    info!(total, successful, failed, "Print request completed");

    PrintResult {
        total,
        successful,
        failed,
        errors,
    }
}

async fn send_to_printer(file_path: &Path, printer_name: &str) -> Result<(), String> {
    let path_str = file_path
        .to_str()
        .ok_or_else(|| "Invalid file path encoding".to_string())?
        .to_string();
    let name = printer_name.to_string();

    tokio::task::spawn_blocking(move || {
        let printer = printers::get_printer_by_name(&name)
            .ok_or_else(|| format!("Printer '{}' not found", name))?;

        printer
            .print_file(&path_str, PrinterJobOptions::none())
            .map_err(|e| format!("Print failed: {:?}", e))?;

        Ok(())
    })
    .await
    .map_err(|e| format!("Print task panicked: {}", e))?
}
