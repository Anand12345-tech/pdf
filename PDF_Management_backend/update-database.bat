@echo off
echo Updating PDF Management database...
cd /d %~dp0
dotnet ef database update --project PdfManagement.Infrastructure --startup-project PdfManagement.API
pause
