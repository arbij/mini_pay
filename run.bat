@echo off

pushd "payment provider server"
start run.bat
popd

pushd server
start run.bat
popd

client.html