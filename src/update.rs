use tracing::{info, warn};

const VERSION: &str = env!("CARGO_PKG_VERSION");

pub fn auto_update() {
    info!("Checking for updates (current version: v{})...", VERSION);

    let result = self_update::backends::github::Update::configure()
        .repo_owner("MusikzugSN")
        .repo_name("VM-PrintService")
        .bin_name("VM-PrintService")
        .show_download_progress(false)
        .no_confirm(true)
        .current_version(VERSION)
        .build();

    match result {
        Ok(updater) => match updater.update() {
            Ok(status) => {
                if status.updated() {
                    info!(
                        "Updated from v{} to v{}. Restarting...",
                        VERSION,
                        status.version()
                    );
                    std::process::exit(0);
                } else {
                    info!("Already up to date (v{}).", VERSION);
                }
            }
            Err(e) => {
                warn!(
                    "Auto-update failed (continuing with current version): {}",
                    e
                );
            }
        },
        Err(e) => {
            warn!(
                "Could not configure auto-update (continuing with current version): {}",
                e
            );
        }
    }
}
