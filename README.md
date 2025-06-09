# PDF Management System

A comprehensive web application for managing, sharing, and collaborating on PDF documents. This system consists of a React frontend and a .NET 8 backend API, providing a complete solution for PDF document management.

## System Architecture

The PDF Management System follows a modern client-server architecture:

- **Frontend**: React-based single-page application (SPA)
- **Backend**: .NET 8 RESTful API with clean architecture
- **Database**: PostgreSQL for data persistence
- **Storage**: Local file system or Google Drive integration
- **Authentication**: JWT-based with OAuth 2.0 support

## Key Features

- **Document Management**
  - Upload, view, download, and delete PDF documents
  - Document organization and search
  - PDF viewing with pagination

- **Sharing System**
  - Generate secure sharing links with expiration dates
  - Public access to shared documents without authentication
  - Access control for shared documents

- **Collaboration**
  - Comment system with replies on specific pages
  - Real-time updates for collaborative editing
  - Notification system for document changes

- **User Management**
  - User registration and authentication
  - OAuth integration with Google
  - Role-based access control
  - User settings and preferences

- **Storage Options**
  - Local file system storage
  - Google Drive integration for cloud storage
  - Configurable storage providers

## Technology Stack

### Frontend
- **Framework**: React 18
- **State Management**: React Context API
- **Routing**: React Router v6
- **UI Components**: Material UI & Bootstrap
- **HTTP Client**: Axios
- **Form Handling**: Formik with Yup validation
- **PDF Rendering**: react-pdf
- **Authentication**: JWT with OAuth 2.0
- **Date Handling**: date-fns

### Backend
- **Framework**: .NET 8 Web API
- **Architecture**: Clean Architecture pattern
- **ORM**: Entity Framework Core 8
- **Database**: PostgreSQL
- **Authentication**: JWT Bearer tokens
- **Documentation**: Swagger/OpenAPI
- **Cloud Storage**: Google Drive API
- **Health Monitoring**: Health checks and diagnostics
- **Validation**: FluentValidation

## Project Structure

```
/PDF_Management_System
├── /PDF_Management_frontend     # React frontend application
│   ├── /public                  # Static assets
│   ├── /src                     # Source code
│   │   ├── /components          # Reusable UI components
│   │   ├── /contexts            # React contexts for state management
│   │   ├── /pages               # Page components
│   │   ├── /services            # API service functions
│   │   └── /utils               # Utility functions
│   ├── package.json             # NPM dependencies and scripts
│   └── .env.example             # Environment variables template
│
├── /PDF_Management_backend      # .NET 8 backend application
│   ├── /PdfManagement.API       # API controllers and endpoints
│   ├── /PdfManagement.Core      # Domain entities and business logic
│   ├── /PdfManagement.Data      # Data access and repositories
│   ├── /PdfManagement.Models    # Shared models and DTOs
│   ├── /PdfManagement.Services  # Service implementations
│   ├── /PdfManagement.Tests     # Unit and integration tests
│   └── PdfManagement.sln        # Solution file
│
├── restart-services.sh          # Script to restart services (Linux/Mac)
└── restart-services.bat         # Script to restart services (Windows)
```

## Setup and Installation

### Prerequisites
- Node.js (v14 or higher)
- .NET 8.0 SDK
- PostgreSQL database
- Google Cloud Platform account (for Google Drive integration)

### Frontend Setup
1. Navigate to the frontend directory:
   ```
   cd PDF_Management_frontend
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Create a `.env` file based on the example:
   ```
   cp .env.example .env
   ```

4. Update the `.env` file with your backend API URL and other settings.

5. Start the development server:
   ```
   npm start
   ```

### Backend Setup
1. Navigate to the backend directory:
   ```
   cd PDF_Management_backend
   ```

2. Create a `.env` file in the root directory with the required environment variables (see backend README for details).

3. Run the database migrations:
   ```
   # Windows
   create-migration.bat
   update-database.bat
   
   # Linux/Mac
   ./create-migration.sh
   ./update-database.sh
   ```

4. Start the API:
   ```
   dotnet run --project PdfManagement.API/PdfManagement.API.csproj
   ```

## Authentication Flow

The system supports two authentication methods:

1. **JWT-based Authentication**:
   - User registers or logs in with email/password
   - Backend validates credentials and issues a JWT token
   - Frontend stores the token in localStorage
   - Token is included in the Authorization header for API requests
   - Protected routes check for valid token

2. **OAuth 2.0 with Google**:
   - User clicks "Sign in with Google" button
   - User is redirected to Google's authentication page
   - After successful authentication, Google redirects back with an authorization code
   - Backend exchanges the code for access and refresh tokens
   - Backend creates or updates the user account and issues a JWT token
   - Frontend stores the JWT token and proceeds as with regular authentication

## Deployment

### Frontend Deployment (Vercel)
1. Connect your GitHub repository to Vercel
2. Configure environment variables in Vercel dashboard
3. Deploy with the following settings:
   - Framework Preset: Create React App
   - Build Command: `npm run build`
   - Output Directory: `build`

### Backend Deployment (Render)
1. Connect your GitHub repository to Render
2. Create a Web Service with the following settings:
   - Build Command: `dotnet publish -c Release`
   - Start Command: `dotnet PdfManagement.API.dll`
   - Environment Variables: Configure as specified in the backend README

## Security Considerations

- JWT tokens are used for secure authentication
- CORS is configured to allow only specific origins
- File uploads are restricted to PDF format with size limits
- API rate limiting is implemented for public endpoints
- All API endpoints are protected with appropriate authorization
- Secure headers are configured for frontend deployment
- Environment variables are used for sensitive configuration

## Monitoring and Maintenance

- Health check endpoints for monitoring system status
- Logging for tracking errors and system activity
- Database migrations for schema updates
- Automated deployment pipelines

## Contributing

1. Create a feature branch from `main`
2. Make your changes
3. Run tests to ensure functionality
4. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
