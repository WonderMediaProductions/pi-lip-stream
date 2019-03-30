@echo off

echo Building...
dotnet publish -r linux-arm /p:ShowLinkerSizeComparison=true 
if ERRORLEVEL 1 goto :error

echo Copying...
REM "c:\Program Files\PuTTY\plink.exe" -i %~DP0..\usr\private-key.ppk pi@%TARGET_PI% -batch -T "rsync -rtv -a /mnt/pi-lip-client/Debug/netcoreapp2.2/linux-arm/publish/* ~/Desktop/pi-lip-client/"
xcopy /y /s /e /d "%~DP0\bin\Debug\netcoreapp2.2\linux-arm\publish" "\\%TARGET_PI%\pi-desktop\pi-lip-client\"
if ERRORLEVEL 1 goto :error
exit /B 0

:error
exit /B 100

REM pushd .\bin\Debug\netcoreapp2.2\linux-arm\publish
REM scp -r  .\* pi@rp3:/home/pi/Desktop/Lipstream/
REM popd
