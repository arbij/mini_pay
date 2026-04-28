@echo off

pushd "set up database"
call run.bat
popd

pushd "test database\set up"
call run.bat