@echo off

pushd "test database/reset"
start run.bat
popd

pushd "payment provider server"
start run.bat
popd

pushd server
start "" "run test.bat"
popd

pushd tests
start run.bat