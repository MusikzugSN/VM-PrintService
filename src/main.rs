use tracing::info;
use tokio_util::sync::CancellationToken;

mod service;
mod update;
mod print;

#[cfg(windows)]
#[macro_use]
extern crate windows_service;

#[cfg(windows)]
mod windows_service_impl;

#[cfg(unix)]
mod linux_service;

const VERSION: &str = env!("CARGO_PKG_VERSION");

fn init_tracing(_foreground: bool) -> Option<tracing_appender::non_blocking::WorkerGuard> {
    let env_filter = tracing_subscriber::EnvFilter::try_from_default_env()
        .unwrap_or_else(|_| tracing_subscriber::EnvFilter::new("info"));

    #[cfg(windows)]
    if !_foreground {
        let log_dir = std::path::PathBuf::from(r"C:\ProgramData\VM-PrintService\logs");
        std::fs::create_dir_all(&log_dir).expect("Failed to create log directory");

        let file_appender = tracing_appender::rolling::daily(&log_dir, "vm-printservice.log");
        let (non_blocking, guard) = tracing_appender::non_blocking(file_appender);

        tracing_subscriber::fmt()
            .with_env_filter(env_filter)
            .with_target(true)
            .with_ansi(false)
            .with_writer(non_blocking)
            .init();

        return Some(guard);
    }

    tracing_subscriber::fmt()
        .with_env_filter(env_filter)
        .with_target(true)
        .init();

    None
}

fn main() {
    let args: Vec<String> = std::env::args().collect();
    let foreground = args.iter().any(|a| a == "--foreground" || a == "-f");

    let _log_guard = init_tracing(foreground);

    info!("VM-PrintService v{}", VERSION);
    update::auto_update();
    let runtime = tokio::runtime::Runtime::new().expect("Failed to create Tokio runtime");

    if foreground {
        info!("Starting in foreground mode");

        let cancel_token = CancellationToken::new();
        let cancel_token_clone = cancel_token.clone();

        runtime.spawn(async move {
            tokio::signal::ctrl_c().await.ok();
            info!("Ctrl+C received, shutting down...");
            cancel_token_clone.cancel();
        });

        runtime.block_on(service::run_service_loop(cancel_token));
        return;
    }

    #[cfg(windows)]
    {
        if let Err(e) = windows_service_impl::run(runtime) {
            eprintln!("Windows service error: {}", e);
            eprintln!("Hint: Use --foreground for foreground mode");
            std::process::exit(1);
        }
    }

    #[cfg(unix)]
    {
        if let Err(e) = linux_service::run(runtime) {
            eprintln!("Linux service error: {}", e);
            std::process::exit(1);
        }
    }
}
