use tracing::info;
use tokio_util::sync::CancellationToken;

mod service;
mod update;

#[cfg(windows)]
#[macro_use]
extern crate windows_service;

#[cfg(windows)]
mod windows_service_impl;

#[cfg(unix)]
mod linux_service;

const VERSION: &str = env!("CARGO_PKG_VERSION");

fn main() {
    tracing_subscriber::fmt()
        .with_env_filter(
            tracing_subscriber::EnvFilter::try_from_default_env()
                .unwrap_or_else(|_| tracing_subscriber::EnvFilter::new("info")),
        )
        .with_target(true)
        .init();

    info!("VM-PrintService v{}", VERSION);
    update::auto_update();

    let args: Vec<String> = std::env::args().collect();
    let runtime = tokio::runtime::Runtime::new().expect("Failed to create Tokio runtime");

    if args.iter().any(|a| a == "--foreground" || a == "-f") {
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
