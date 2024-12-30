use std::collections::HashMap;
use std::io;
use winreg::enums::*;
use winreg::types::*;
use winreg::RegKey;

const IMAGE_OPTIONS: &str = r"SOFTWARE\Processes Priority Manager\Image Options";
const AFFINITY_VALUE_NAME: &str = "Affinity";

pub(crate) fn get_affinity_masks() -> io::Result<HashMap<String, u64>> {
    let mut affinity_masks = HashMap::new();

    let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
    let affinity_keys = match hklm.open_subkey(IMAGE_OPTIONS) {
        Ok(keys) => keys,
        Err(e) => match e.kind() {
            io::ErrorKind::NotFound => return Ok(affinity_masks),
            _ => return Err(e),
        },
    };

    for mut name in affinity_keys.enum_keys().map(|x| x.unwrap()) {
        let sub_key = affinity_keys.open_subkey(&name).unwrap();

        let value: u64 = match sub_key.get_raw_value(AFFINITY_VALUE_NAME) {
            Ok(value) => match value.vtype {
                REG_QWORD => FromRegValue::from_reg_value(&value).unwrap(),
                _ => continue,
            },
            Err(e) => match e.kind() {
                io::ErrorKind::NotFound => continue,
                _ => return Err(e),
            },
        };
        name = name.to_lowercase();
        affinity_masks.insert(name, value);
    }

    affinity_masks.shrink_to_fit();

    Ok(affinity_masks)
}
