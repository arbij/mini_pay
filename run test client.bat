@echo off

start "" "http://localhost:8000/client.html?port=5002"

python -m http.server 8000 --directory .

pause