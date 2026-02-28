use tracing::{error, info};
use std::ffi::OsString;
use std::sync::OnceLock;
use std::time::Duration;
use tokio_util::sync::CancellationToken;
use windows_service::service::{
    ServiceControl, ServiceControlAccept, ServiceExitCode, ServiceState, ServiceStatus, ServiceType,
};
use windows_service::service_control_handler::{self, ServiceControlHandlerResult};
use windows_service::service_dispatcher;

use crate::service::run_service_loop;

const SERVICE_NAME: &str = "VMPrintService";
const SERVICE_TYPE: ServiceType = ServiceType::OWN_PROCESS;

static RUNTIME: OnceLock<tokio::runtime::Runtime> = OnceLock::new();

pub fn run(runtime: tokio::runtime::Runtime) -> Result<(), String> {
    RUNTIME.set(runtime).map_err(|_| "Runtime already set".to_string())?;
    service_dispatcher::start(SERVICE_NAME, ffi_service_main)
        .map_err(|e| format!("Failed to start service dispatcher: {}", e))
}

define_windows_service!(ffi_service_main, service_main);

fn service_main(args: Vec<OsString>) {
    if let Err(e) = run_service_inner(args) {
        error!("Service error: {}", e);
    }
}

fn run_service_inner(_args: Vec<OsString>) -> Result<(), String> {
    let cancel_token = CancellationToken::new();
    let cancel_token_clone = cancel_token.clone();

    let event_handler = move |control_event| -> ServiceControlHandlerResult {
        match control_event {
            ServiceControl::Stop => {
                info!("Stop signal received");
                cancel_token_clone.cancel();
                ServiceControlHandlerResult::NoError
            }
            ServiceControl::Interrogate => ServiceControlHandlerResult::NoError,
            _ => ServiceControlHandlerResult::NotImplemented,
        }
    };

    let status_handle = service_control_handler::register(SERVICE_NAME, event_handler)
        .map_err(|e| format!("Failed to register control handler: {}", e))?;

    status_handle
        .set_service_status(ServiceStatus {
            service_type: SERVICE_TYPE,
            current_state: ServiceState::Running,
            controls_accepted: ServiceControlAccept::STOP,
            exit_code: ServiceExitCode::Win32(0),
            checkpoint: 0,
            wait_hint: Duration::default(),
            process_id: None,
        })
        .map_err(|e| format!("Failed to set service status: {}", e))?;

    info!("Windows service started");

    let runtime = RUNTIME.get().expect("Runtime not initialized");
    runtime.block_on(run_service_loop(cancel_token));

    status_handle
        .set_service_status(ServiceStatus {
            service_type: SERVICE_TYPE,
            current_state: ServiceState::Stopped,
            controls_accepted: ServiceControlAccept::empty(),
            exit_code: ServiceExitCode::Win32(0),
            checkpoint: 0,
            wait_hint: Duration::default(),
            process_id: None,
        })
        .map_err(|e| format!("Failed to set service status: {}", e))?;

    info!("Windows service stopped");
    Ok(())
}
