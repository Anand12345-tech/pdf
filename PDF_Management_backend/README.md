# PDF Management Backend

A .NET 8 application for managing PDF documents with features like uploading, sharing, and commenting.

## Architecture Overview

The application follows a clean architecture pattern with the following layers:

- **API Layer** (`PdfManagement.API`): REST API endpoints, controllers, and request/response models
- **Core Layer** (`PdfManagement.Core`): Domain entities, interfaces, and core business logic
- **Infrastructure Layer** (`PdfManagement.Infrastructure`): Database context, migrations, and repository implementations
- **Services Layer** (`PdfManagement.Services`): Business logic implementation and external service integrations

## Key Features

- **Document Management**: Upload, view, download, and delete PDF documents
- **Sharing System**: Generate secure sharing links with expiration dates
- **Comment System**: Add and view comments on specific pages of PDF documents
- **Authentication**: JWT-based authentication with user registration and login
- **Google Drive Integration**: Cloud storage for PDF documents
- **Health Checks**: Endpoints to verify application and database health

## Technology Stack

- **Framework**: .NET 8 Web API
- **Database**: PostgreSQL (via Entity Framework Core)
- **Authentication**: JWT Bearer tokens
- **Storage**: Google Drive API for cloud storage
- **Documentation**: Swagger/OpenAPI
- **Deployment**: Render (Backend), Vercel (Frontend)

## Environment Setup

The application uses environment variables for configuration. Create a `.env` file in the root directory with the following variables:

```
# Environment
ASPNETCORE_ENVIRONMENT=Development

# Database Connection
DB_CONNECTION_STRING=Host=your_host;Port=5432;Database=your_db;Username=your_user;Password=your_password;SSL Mode=Prefer;Trust Server Certificate=true

# JWT Settings
JWT_KEY=your_jwt_key_at_least_32_chars_long
JWT_ISSUER=PdfManagement.API
JWT_AUDIENCE=PdfManagementClient
JWT_EXPIRE_DAYS=7

# Storage Settings
FILE_STORAGE_PROVIDER=GoogleDrive
FileStorage__TempPath=temp

# Google Drive Configuration
GOOGLE_DRIVE_FOLDER_ID=your_folder_id
GOOGLE_CLIENT_EMAIL=your_service_account_email
GOOGLE_CLIENT_ID=your_client_id
GOOGLE_PRIVATE_KEY="your_private_key"
GOOGLE_TYPE=service_account
GOOGLE_PROJECT_ID=your_project_id
GOOGLE_AUTH_URI=https://accounts.google.com/o/oauth2/auth
GOOGLE_TOKEN_URI=https://oauth2.googleapis.com/token
GOOGLE_AUTH_PROVIDER_X509_CERT_URL=https://www.googleapis.com/oauth2/v1/certs
GOOGLE_CLIENT_X509_CERT_URL=https://www.googleapis.com/robot/v1/metadata/x509/your_service_account_email

# CORS Settings
ALLOWED_ORIGINS=http://localhost:3000,https://your-frontend-domain.com

# Frontend Settings
FRONTEND_URL=https://your-frontend-domain.com
```

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

1. Create a new migration:
   ```
   dotnet ef migrations add MigrationName --project PdfManagement.Infrastructure --startup-project PdfManagement.API --output-dir Migrations
   ```

2. Apply the migration to update the database:
   ```
   dotnet ef database update --project PdfManagement.Infrastructure --startup-project PdfManagement.API
   ```

## Running the Application

After setting up the database, you can run the application:

- Development: `dotnet run --project PdfManagement.API/PdfManagement.API.csproj`
- With specific URLs: `dotnet run --project PdfManagement.API/PdfManagement.API.csproj --urls="http://0.0.0.0:5000;https://0.0.0.0:5001"`
- Production build: `dotnet publish -c Release`

## API Endpoints

The API provides the following main endpoints:

- **Authentication**:
  - `POST /api/Auth/register`: Register a new user
  - `POST /api/Auth/login`: Login and get JWT token

- **PDF Documents**:
  - `GET /api/PdfDocuments`: Get all user documents
  - `GET /api/PdfDocuments/{id}`: Get document by ID
  - `POST /api/PdfDocuments`: Upload a new document
  - `DELETE /api/PdfDocuments/{id}`: Delete a document
  - `POST /api/PdfDocuments/{id}/share`: Generate a sharing link
  - `GET /api/PdfDocuments/download/{id}`: Download a document
  - `GET /api/PdfDocuments/view/{id}`: View a document

- **Public Access**:
  - `GET /api/Public/view/{token}`: View a shared document
  - `GET /api/Public/download/{token}`: Download a shared document
  - `POST /api/Public/comment/{token}`: Add a comment to a shared document

- **Comments**:
  - `GET /api/Comments/document/{documentId}`: Get comments for a document
  - `POST /api/Comments`: Add a comment to a document
  - `PUT /api/Comments/{id}`: Update a comment
  - `DELETE /api/Comments/{id}`: Delete a comment

- **Health Checks**:
  - `GET /health`: Simple health check
  - `GET /health/ready`: Readiness check (includes database)
  - `GET /health/live`: Liveness check

## Deployment

The backend is configured for deployment on Render with the following settings:

- **Build Command**: `dotnet publish -c Release`
- **Start Command**: `dotnet PdfManagement.API.dll`
- **Environment Variables**: Configure as specified in the Environment Setup section

## Security Considerations

- JWT tokens are used for authentication
- CORS is configured to allow only specific origins
- Rate limiting is implemented for public endpoints
- File uploads are restricted to PDF format and reasonable size limits
- Google Drive API uses service account authentication

## Troubleshooting

- **Database Connection Issues**: Verify connection string and database server accessibility
- **Storage Issues**: Check Google Drive API credentials and permissions
- **CORS Errors**: Ensure the frontend domain is added to the `ALLOWED_ORIGINS` environment variable
- **JWT Authentication Failures**: Verify the JWT settings and token expiration

## Contributing

1. Create a feature branch from `main`
2. Make your changes
3. Run tests to ensure functionality
4. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
