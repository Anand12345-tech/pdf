# PDF Management Application

A .NET 8 application for managing PDF documents with features like uploading, sharing, and commenting.

## Database Setup

Before running the application, you need to set up the database:

### Option 1: Using the Scripts

1. Run the migration creation script:
   - Windows: Double-click `create-migration.bat`
   - Linux/Mac: Run `./create-migration.sh`

2. Apply the migration to create the database:
   - Windows: Double-click `update-database.bat`
   - Linux/Mac: Run `./update-database.sh`

### Option 2: Using the Command Line

1. Create the initial migration:
   ```
   dotnet ef migrations add InitialCreate --project PdfManagement.Infrastructure --startup-project PdfManagement.API --output-dir Data/Migrations
   ```

2. Apply the migration to create the database:
   ```
   dotnet ef database update --project PdfManagement.Infrastructure --startup-project PdfManagement.API
   ```

## Running the Application

After setting up the database, you can run the application:

- Windows: Double-click `run-api.bat`
- Linux/Mac: Run `./run-api.sh`
- Command Line: `dotnet run --project PdfManagement.API/PdfManagement.API.csproj --urls="http://0.0.0.0:5000;https://0.0.0.0:5001"`

## Accessing the API

- Swagger UI: http://localhost:5000 or https://localhost:5001
- Health Check: http://localhost:5000/health or https://localhost:5001/health

## Default Users

The application is seeded with two default users:

1. Admin User:
   - Email: admin@pdfmanagement.com
   - Password: Admin@123
   - Role: Admin

2. Demo User:
   - Email: demo@pdfmanagement.com
   - Password: Demo@123
   - Role: User

## Connection String

The default connection string is configured for SQL Server:

```
Server=localhost;Database=PdfManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

If you need to use a different database server or credentials, update the connection string in:
- `appsettings.json`
- `appsettings.Development.json`

## Troubleshooting Database Issues

If you encounter database connection issues:

1. Verify SQL Server is running and accessible
2. Check that the connection string is correct for your environment
3. Ensure the database user has appropriate permissions
4. Try recreating the migrations:
   ```
   dotnet ef migrations remove --project PdfManagement.Infrastructure --startup-project PdfManagement.API
   ```
   Then create a new migration and update the database as described above.

5. For SQL Server authentication issues, you can modify the connection string to use SQL authentication:
   ```
   Server=localhost;Database=PdfManagementDb;User Id=YourUsername;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=true
   ```
