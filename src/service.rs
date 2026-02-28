use tracing::info;
use std::time::Duration;
use tokio_util::sync::CancellationToken;

pub async fn run_service_loop(cancel_token: CancellationToken) {
    info!("Service loop started");

    loop {
        tokio::select! {
            _ = cancel_token.cancelled() => {
                break;
            }
            _ = tokio::time::sleep(Duration::from_secs(5)) => {
                info!("Service is running...");
            }
        }
    }

    info!("Service loop stopped");
}
