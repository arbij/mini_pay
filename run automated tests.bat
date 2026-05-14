@echo off

pushd server
start cmd /c "run test.bat"
popd

pushd "payment provider server"
start cmd /c run.bat
popd

pushd "test database/reset"
call run.bat
popd

cd tests
run.bat