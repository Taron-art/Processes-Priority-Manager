use embed_manifest::manifest::SupportedOS::Windows10;
use embed_manifest::{embed_manifest, new_manifest};

fn main() {
    if std::env::var_os("CARGO_CFG_WINDOWS").is_some() {
        embed_manifest(
            new_manifest("default")
                .supported_os(Windows10..=Windows10)
                .remove_dependency("Microsoft.Windows.Common-Controls"),
        )
        .expect("unable to embed manifest file");

        let res = winres::WindowsResource::new();
        res.compile().unwrap();
        static_vcruntime::metabuild(); // link the static vcruntime
    }
    println!("cargo:rerun-if-changed=build.rs");
}
