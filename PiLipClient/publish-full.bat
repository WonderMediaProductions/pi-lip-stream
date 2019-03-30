dotnet publish -r linux-arm /p:ShowLinkerSizeComparison=true 
pushd .\bin\Debug\netcoreapp2.2\linux-arm\publish
scp -r  .\* pi@rp3:/home/pi/Desktop/Lipstream/
popd