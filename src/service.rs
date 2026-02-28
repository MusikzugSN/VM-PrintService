use actix_cors::Cors;
use actix_web::{App, HttpResponse, HttpServer, web};
use tokio_util::sync::CancellationToken;
use tracing::info;

use crate::print;

const LISTEN_ADDR: &str = "127.0.0.1";
const LISTEN_PORT: u16 = 1913;

async fn get_printers() -> HttpResponse {
    match print::list_printers().await {
        Ok(printers) => HttpResponse::Ok().json(printers),
        Err(e) => HttpResponse::InternalServerError().json(serde_json::json!({
            "error": e
        })),
    }
}

async fn post_print(request: web::Json<print::PrintRequest>) -> HttpResponse {
    let request = request.into_inner();

    if request.files.is_empty() {
        return HttpResponse::BadRequest().json(serde_json::json!({
            "error": "No files provided"
        }));
    }

    if request.printer.is_empty() {
        return HttpResponse::BadRequest().json(serde_json::json!({
            "error": "No printer specified"
        }));
    }

    let result = print::process_print_request(request).await;

    if result.failed > 0 {
        HttpResponse::Ok().json(result)
    } else {
        HttpResponse::Ok().json(result)
    }
}

async fn health() -> HttpResponse {
    HttpResponse::Ok().json(serde_json::json!({
        "status": "ok",
        "version": env!("CARGO_PKG_VERSION")
    }))
}

pub async fn run_service_loop(cancel_token: CancellationToken) {
    info!(
        "Starting Actix web server on {}:{}",
        LISTEN_ADDR, LISTEN_PORT
    );

    let server = HttpServer::new(move || {
        let cors = Cors::default()
            .allow_any_origin()
            .allow_any_method()
            .allow_any_header()
            .max_age(3600);

        App::new()
            .wrap(cors)
            .route("/api/health", web::get().to(health))
            .route("/api/printers", web::get().to(get_printers))
            .route("/api/print", web::post().to(post_print))
    })
    .bind((LISTEN_ADDR, LISTEN_PORT))
    .expect("Failed to bind Actix web server")
    .disable_signals()
    .shutdown_timeout(5)
    .run();

    let server_handle = server.handle();

    let handle = server_handle.clone();
    tokio::spawn(async move {
        cancel_token.cancelled().await;
        info!("Shutdown signal received, stopping web server...");
        handle.stop(true).await;
    });

    info!(
        "Service loop started — listening on http://{}:{}",
        LISTEN_ADDR, LISTEN_PORT
    );

    server.await.ok();

    info!("Service loop stopped");
}
