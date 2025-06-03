#!/bin/bash
echo "Updating PDF Management database..."
cd "$(dirname "$0")"
dotnet ef database update --project PdfManagement.Infrastructure --startup-project PdfManagement.API
