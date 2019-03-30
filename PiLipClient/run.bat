@echo off

echo Running...
"c:\Program Files\PuTTY\plink.exe" -i %~DP0..\usr\private-key.ppk pi@%TARGET_PI% -batch -T "/bin/dotnet/dotnet ~/Desktop/pi-lip-client/PiLipClient.dll"
if ERRORLEVEL 1 goto :error

REM echo Playing...
REM "c:\Program Files\PuTTY\plink.exe" -i %~DP0..\usr\private-key.ppk pi@%TARGET_PI% -batch -T "export DISPLAY=:0; nohup vlc --play-and-exit -f ~/Desktop/output.h264 &>/dev/null"
if ERRORLEVEL 1 goto :error
