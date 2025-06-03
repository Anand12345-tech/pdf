#!/bin/bash
echo "Creating initial migration for PDF Management database..."
cd "$(dirname "$0")"
dotnet ef migrations add InitialCreate --project PdfManagement.Infrastructure --startup-project PdfManagement.API --output-dir Data/Migrations
echo ""
echo "Migration created. To apply the migration, run the application or use:"
echo "dotnet ef database update --project PdfManagement.Infrastructure --startup-project PdfManagement.API"
