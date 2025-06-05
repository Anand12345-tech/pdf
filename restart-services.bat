@echo off
echo Stopping any running services...
taskkill /F /IM dotnet.exe /T 2>nul
taskkill /F /IM node.exe /T 2>nul

echo Starting API service...
start cmd /k "cd /d D:\pdf_management\repo\PDF_Management.Net-8\PdfManagement.API && dotnet run"

echo Waiting for API to initialize (10 seconds)...
timeout /t 10 /nobreak

echo Starting frontend service...
start cmd /k "cd /d D:\pdf_management\repo\frontend && npm run start-win"

echo Services started!
echo.
echo To stop services, run: taskkill /F /IM dotnet.exe /T && taskkill /F /IM node.exe /T
