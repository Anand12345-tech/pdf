# PDF Management System

A web application for managing, sharing, and collaborating on PDF documents.

## Setup Instructions

### Prerequisites
- Node.js (v14 or higher)
- .NET 8.0 SDK
- SQL Server (or compatible database)

### Frontend Setup
1. Navigate to the frontend directory:
   ```
   cd frontend
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Create a `.env` file based on the example:
   ```
   cp .env.example .env
   ```

4. Start the development server:
   ```
   npm start
   ```

### Backend Setup
1. Navigate to the backend directory:
   ```
   cd PDF_Management.Net-8
   ```

2. Create `appsettings.json` and `appsettings.Development.json` based on the template:
   ```
   cp PdfManagement.API/appsettings.Template.json PdfManagement.API/appsettings.json
   cp PdfManagement.API/appsettings.Template.json PdfManagement.API/appsettings.Development.json
   ```

3. Update the configuration files with your database connection string and other settings.

4. Run the migrations to set up the database:
   ```
   dotnet ef database update --project PdfManagement.Data --startup-project PdfManagement.API
   ```

5. Start the API:
   ```
   dotnet run --project PdfManagement.API
   ```

## Features
- PDF document upload and management
- Document sharing with expiration dates
- Commenting and collaboration on shared documents
- User authentication and authorization

## Project Structure
- `/frontend` - React frontend application
- `/PDF_Management.Net-8` - .NET 8 backend API
  - `/PdfManagement.API` - API controllers and endpoints
  - `/PdfManagement.Core` - Core domain models and business logic
  - `/PdfManagement.Data` - Data access and database context
  - `/PdfManagement.Services` - Service implementations
  - `/PdfManagement.Tests` - Unit and integration tests

## Contributing
1. Create a feature branch from `main`
2. Make your changes
3. Submit a pull request

## License
[MIT](LICENSE)
