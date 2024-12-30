use serde::Deserialize;
use std::collections::HashMap;
use windows::Win32::{Foundation::CloseHandle, System::Threading};
use wmi::{COMLibrary, WMIConnection};

#[derive(Deserialize, Debug)]
#[serde(rename = "Win32_ProcessStartTrace")]
#[serde(rename_all = "PascalCase")]
struct ProcessStartTrace {
    process_id: u32,
    process_name: String,
}

pub fn run_loop(affinity_masks: &HashMap<String, u64>) {
    let com_con = COMLibrary::new().unwrap();
    let wmi_con = WMIConnection::new(com_con).unwrap();
    let iterator = wmi_con.notification::<ProcessStartTrace>().unwrap();
    for result in iterator {
        let trace = result.unwrap();
        let value = affinity_masks.get(&trace.process_name.to_lowercase());

        if value.is_some() {
            unsafe {
                let process_handle = match Threading::OpenProcess(
                    Threading::PROCESS_SET_INFORMATION | Threading::PROCESS_QUERY_INFORMATION,
                    false,
                    trace.process_id,
                ) {
                    Ok(handle) => handle,
                    Err(_) => continue, // The process was probably stopped before we could set the affinity mask.
                };

                let mut system_affinity_mask: usize = 0;
                let mut current_affinity: usize = 0;

                _ = Threading::GetProcessAffinityMask(
                    process_handle,
                    &mut current_affinity as *mut usize,
                    &mut system_affinity_mask as *mut usize,
                );

                let affinity_mask_to_apply = *value.unwrap() as usize & system_affinity_mask;
                if affinity_mask_to_apply != current_affinity {
                    _ = Threading::SetProcessAffinityMask(process_handle, affinity_mask_to_apply);
                }
                _ = CloseHandle(process_handle);
            }
        };
    } // Loop will end only on error
}
