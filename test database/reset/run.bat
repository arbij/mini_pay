@echo off

sqlcmd -S localhost -C -d mini_pay_test -Y 22 -i sql.sql

echo test database reset successfully

pause