@echo off
"c:\Program Files\PuTTY\plink.exe" -i %~DP0..\usr\private-key.ppk pi@%TARGET_PI% -batch -T "rsync -rtv -a /mnt/pi-lip-client/Debug/netcoreapp2.2/linux-arm/publish/* ~/Desktop/pi-lip-client/"
