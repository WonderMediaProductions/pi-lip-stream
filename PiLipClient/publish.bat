@echo off

dotnet publish -r linux-arm /p:ShowLinkerSizeComparison=true 
if ERRORLEVEL 1 goto :error

call %~DP0deploy
if ERRORLEVEL 1 goto :error

exit /B 0

:error
exit /B 100

REM pushd .\bin\Debug\netcoreapp2.2\linux-arm\publish
REM scp -r  .\* pi@rp3:/home/pi/Desktop/Lipstream/
REM popd
