@echo off

sqlcmd -S localhost -C -Y 22 -i sql.sql

pause