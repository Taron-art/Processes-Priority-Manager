use log::{error, info};
use register::SERVICE_NAME;
use simplelog::*;
use std::{collections::HashMap, thread, time::Duration};
use windows_service::{
    define_windows_service,
    service::{
        ServiceControl, ServiceControlAccept, ServiceExitCode, ServiceState, ServiceStatus,
        ServiceType,
    },
    service_control_handler::{self, ServiceControlHandlerResult},
    service_dispatcher, Result,
};
use windows::Win32::{self, Foundation::CloseHandle, System::Threading};

mod affinity_getter;
mod register;
mod wmi_watcher;

fn main() {
    if std::env::args().any(|arg| arg == "install") {
        register::install_service();
        return;
    }

    if std::env::args().any(|arg| arg == "uninstall") {
        register::uninstall_service();
        return;
    }

    if let Err(e) = service_dispatcher::start(SERVICE_NAME, ffi_service_main) {
        eprintln!("Failed to start service: {:?}", e);
    }
}

// Main service entry point
define_windows_service!(ffi_service_main, service_main);
fn service_main(_arguments: Vec<std::ffi::OsString>) {
    if let Err(e) = run_service() {
        error!("Service failed: {:?}", e);
    }
}

fn run_service() -> Result<()> {
    // Set up logging
    SimpleLogger::init(LevelFilter::Info, Config::default()).expect("Failed to initialize logger");
    info!("Starting Windows Service...");

    // Define service status
    let (shutdown_tx, shutdown_rx) = std::sync::mpsc::channel();
    let status_handle =
        service_control_handler::register(
            SERVICE_NAME,
            move |control_event| match control_event {
                ServiceControl::Stop => {
                    shutdown_tx.send(()).unwrap();
                    ServiceControlHandlerResult::NoError
                }
                _ => ServiceControlHandlerResult::NotImplemented,
            },
        )?;

    let affinity_masks: HashMap<String, u64> = affinity_getter::get_affinity_masks().unwrap();
    if !affinity_masks.is_empty() {
        status_handle.set_service_status(ServiceStatus {
            service_type: ServiceType::OWN_PROCESS,
            current_state: ServiceState::Running,
            controls_accepted: ServiceControlAccept::STOP,
            exit_code: ServiceExitCode::Win32(0),
            checkpoint: 0,
            wait_hint: Duration::from_secs(10),
            process_id: None,
        })?;

        // Main service loop
        info!("Service is running...");
        let thread_builder = thread::Builder::new().name("WMI Watcher".into());
        let thread_join_handle = thread_builder
            .spawn(move || {
                wmi_watcher::run_loop(&affinity_masks);
            })
            .unwrap();

        release_unused_memory();

        loop {
            if shutdown_rx.try_recv().is_ok() {
                info!("Shutdown signal received");
                break;
            }
            if thread_join_handle.is_finished() {
                panic!("WMI Watcher thread has exited unexpectedly");
            }

            thread::sleep(Duration::from_secs(1));
        }
    } else {
        info!("No affinity masks found. Service will stop.");
    }
    info!("Service is stopping...");
    status_handle.set_service_status(ServiceStatus {
        service_type: ServiceType::OWN_PROCESS,
        current_state: ServiceState::Stopped,
        controls_accepted: ServiceControlAccept::empty(),
        exit_code: ServiceExitCode::Win32(0),
        checkpoint: 0,
        wait_hint: Duration::from_secs(0),
        process_id: None,
    })?;
    Ok(())
}

fn release_unused_memory() {
    unsafe {
        let handle = Threading::GetCurrentProcess();
        _ = Win32::System::ProcessStatus::EmptyWorkingSet(handle);
        _ = CloseHandle(handle);
    }
}
