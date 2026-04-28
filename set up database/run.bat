@echo off

sqlcmd -S localhost -C -i sql.sql -Y 22

pause