#!/bin/bash
echo "Creating PostgreSQL migration for PDF Management database..."
cd "$(dirname "$0")"
dotnet ef migrations add PostgreSQLMigration --project PdfManagement.Infrastructure --startup-project PdfManagement.API --output-dir Migrations
echo ""
echo "PostgreSQL migration created. To apply the migration, run the application or use:"
echo "dotnet ef database update --project PdfManagement.Infrastructure --startup-project PdfManagement.API"
