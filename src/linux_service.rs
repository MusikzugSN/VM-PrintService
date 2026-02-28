use tokio_util::sync::CancellationToken;
use tracing::info;

use crate::service::run_service_loop;

pub fn run(runtime: tokio::runtime::Runtime) -> Result<(), String> {
    let cancel_token = CancellationToken::new();
    let cancel_token_clone = cancel_token.clone();

    runtime.spawn(async move {
        tokio::signal::ctrl_c().await.ok();
        info!("Signal received, shutting down...");
        cancel_token_clone.cancel();
    });

    info!("Linux service started (systemd mode)");

    runtime.block_on(run_service_loop(cancel_token));

    info!("Linux service stopped");
    Ok(())
}
