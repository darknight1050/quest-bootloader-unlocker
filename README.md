# Quest Bootloader Unlocker

This can unlock the bootloader on the Quest 2 if it still runs an old version.

Latest vulnerable version is `16476800118700000 (29.0.0.65.370.289987413)` from `May 9 2021`.

It uses [CVE-2021-1931](https://nvd.nist.gov/vuln/detail/CVE-2021-1931) a buffer overflow in fastboot that got fixed in [this](https://github.com/tianocore/edk2/commit/0727b7b0d4cafb091397b76f75a3a4f66852a361) commit.<br>
This [blogpost](https://www.pentestpartners.com/security-blog/breaking-the-android-bootloader-on-the-qualcomm-snapdragon-660/) by Christopher Wade is about the discovery of this vulnerability.<br>

Using the vulnerability this tool patches the signature checks for unlocking the bootloader and then unlocks it.<br>
The bootloader binary can be extracted with [extract_android_ota_payload](https://github.com/cyxx/extract_android_ota_payload) and then [uefi-firmware-parser](https://github.com/theopolis/uefi-firmware-parser) on the `abl.img`
