# PDF Management Backend

A .NET 8 application for managing PDF documents with features like uploading, sharing, and commenting.

## Architecture Overview

The application follows a clean architecture pattern with the following layers:

- **API Layer** (`PdfManagement.API`): REST API endpoints, controllers, and request/response models
- **Core Layer** (`PdfManagement.Core`): Domain entities, interfaces, and core business logic
- **Data Layer** (`PdfManagement.Data`): Repositories and data access implementations
- **Infrastructure Layer** (`PdfManagement.Infrastructure`): Database context, migrations, and external service integrations
- **Models Layer** (`PdfManagement.Models`): DTOs, view models, and shared data structures
- **Services Layer** (`PdfManagement.Services`): Business logic implementation and service interfaces

## Key Features

- **Document Management**: Upload, view, download, and delete PDF documents
- **Sharing System**: Generate secure sharing links with expiration dates
- **Comment System**: Add and view comments on specific pages of PDF documents
- **Authentication**: JWT-based authentication with user registration and login
- **OAuth Integration**: Support for Google OAuth authentication
- **Google Drive Integration**: Cloud storage for PDF documents
- **Health Checks**: Endpoints to verify application and database health
- **Settings Management**: Configurable storage providers and user preferences

## Technology Stack

- **Framework**: .NET 8 Web API
- **Database**: PostgreSQL (via Entity Framework Core 8)
- **ORM**: Entity Framework Core with code-first migrations
- **Authentication**: JWT Bearer tokens with OAuth 2.0 support
- **Storage**: Local file system and Google Drive API
- **Documentation**: Swagger/OpenAPI with annotations
- **Validation**: FluentValidation for request validation
- **Testing**: xUnit with Moq for unit testing
- **Deployment**: Docker containerization with Render hosting

## Project Structure

```
/PdfManagement.API
├── /Controllers          # API endpoints and route definitions
├── /HealthChecks         # Custom health check implementations
├── /Middleware           # Custom middleware components
├── /Models               # API-specific models and DTOs
├── Program.cs            # Application entry point and configuration
└── appsettings.json      # Application settings

/PdfManagement.Core
├── /Application          # Application services and interfaces
├── /Domain               # Domain entities and business rules
└── /Extensions           # Core extension methods

/PdfManagement.Data
├── /Repositories         # Data access implementations
└── /Interfaces           # Repository interfaces

/PdfManagement.Infrastructure
├── /Data                 # Database context and configurations
│   ├── /Context          # EF Core DbContext implementation
│   ├── /Mappings         # Entity type configurations
│   └── /Migrations       # Database migrations
├── /Services             # External service integrations
└── /Extensions           # Infrastructure extension methods

/PdfManagement.Models
├── /Auth                 # Authentication-related models
├── /Comments             # Comment-related models
├── /Common               # Shared models and base classes
├── /Documents            # Document-related models
├── /Entities             # Entity models
├── /Public               # Public access models
└── /Settings             # Settings-related models

/PdfManagement.Services
├── /Implementations      # Service implementations
└── /Interfaces           # Service interfaces

/PdfManagement.Tests
├── /API                  # API controller tests
├── /Services             # Service implementation tests
└── /Infrastructure       # Infrastructure component tests
```

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
FILE_STORAGE_PROVIDER=Local
FileStorage__LocalBasePath=./uploads
FileStorage__TempPath=./temp

# Google Drive Configuration (if using Google Drive storage)
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

# Google OAuth Settings (for user authentication)
GOOGLE_OAUTH_CLIENT_ID=your_oauth_client_id
GOOGLE_OAUTH_CLIENT_SECRET=your_oauth_client_secret
GOOGLE_OAUTH_REDIRECT_URI=https://your-frontend-domain.com/oauth-success

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
   dotnet ef migrations add MigrationName --project PdfManagement.Infrastructure --startup-project PdfManagement.API --output-dir Data/Migrations
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
- Docker: `docker-compose up`

## API Endpoints

The API provides the following main endpoints:

### Authentication

- `POST /api/Auth/register`: Register a new user
  ```json
  {
    "email": "user@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe"
  }
  ```

- `POST /api/Auth/login`: Login and get JWT token
  ```json
  {
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }
  ```

- `POST /api/Auth/google-login`: Login with Google OAuth token
  ```json
  {
    "token": "google_id_token"
  }
  ```

### PDF Documents

- `GET /api/PdfDocuments`: Get all user documents
- `GET /api/PdfDocuments/{id}`: Get document by ID
- `POST /api/PdfDocuments`: Upload a new document (multipart/form-data)
- `DELETE /api/PdfDocuments/{id}`: Delete a document
- `POST /api/PdfDocuments/{id}/share`: Generate a sharing link
  ```json
  {
    "expirationDays": 7,
    "allowComments": true,
    "allowDownload": true
  }
  ```
- `GET /api/PdfDocuments/download/{id}`: Download a document
- `GET /api/PdfDocuments/view/{id}`: View a document

### Public Access

- `GET /api/Public/view/{token}`: View a shared document
- `GET /api/Public/download/{token}`: Download a shared document
- `POST /api/Public/comment/{token}`: Add a comment to a shared document
  ```json
  {
    "pageNumber": 1,
    "content": "This is a comment",
    "x": 100,
    "y": 200
  }
  ```

### Comments

- `GET /api/Comments/document/{documentId}`: Get comments for a document
- `POST /api/Comments`: Add a comment to a document
  ```json
  {
    "documentId": "document_id",
    "pageNumber": 1,
    "content": "This is a comment",
    "x": 100,
    "y": 200
  }
  ```
- `PUT /api/Comments/{id}`: Update a comment
  ```json
  {
    "content": "Updated comment"
  }
  ```
- `DELETE /api/Comments/{id}`: Delete a comment

### Settings

- `GET /api/Settings/storage`: Get storage settings
- `PUT /api/Settings/storage`: Update storage settings (Admin only)
  ```json
  {
    "provider": "GoogleDrive",
    "localBasePath": "/custom/path"
  }
  ```

### Health Checks

- `GET /health`: Simple health check
- `GET /health/ready`: Readiness check (includes database)
- `GET /health/live`: Liveness check

## Authentication Flow

The system supports two authentication methods:

1. **JWT-based Authentication**:
   - User registers or logs in with email/password
   - Backend validates credentials and issues a JWT token
   - Token is included in the Authorization header for API requests
   - Protected routes check for valid token

2. **OAuth 2.0 with Google**:
   - Frontend obtains Google ID token
   - Backend validates the token with Google
   - Backend creates or updates the user account and issues a JWT token
   - Authentication proceeds as with regular JWT authentication

## Deployment

### Docker Deployment

1. Build the Docker image:
   ```
   docker build -t pdf-management-api .
   ```

2. Run the container:
   ```
   docker run -p 5000:80 --env-file .env pdf-management-api
   ```

### Render Deployment

The backend is configured for deployment on Render with the following settings:

- **Build Command**: `dotnet publish -c Release`
- **Start Command**: `dotnet PdfManagement.API.dll`
- **Environment Variables**: Configure as specified in the Environment Setup section

## Security Considerations

- JWT tokens are used for secure authentication
- Password hashing with ASP.NET Core Identity
- CORS is configured to allow only specific origins
- Rate limiting is implemented for public endpoints
- File uploads are restricted to PDF format and reasonable size limits
- Google Drive API uses service account authentication
- All API endpoints are protected with appropriate authorization

## Troubleshooting

- **Database Connection Issues**: Verify connection string and database server accessibility
- **Storage Issues**: Check file permissions or Google Drive API credentials
- **CORS Errors**: Ensure the frontend domain is added to the `ALLOWED_ORIGINS` environment variable
- **JWT Authentication Failures**: Verify the JWT settings and token expiration
- **PDF Processing Errors**: Check for corrupted PDF files or insufficient memory

## Contributing

1. Create a feature branch from `main`
2. Make your changes
3. Run tests to ensure functionality
4. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
