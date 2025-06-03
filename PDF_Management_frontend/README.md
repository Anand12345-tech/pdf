# PDF Management Frontend

This is a React frontend application for the PDF Management system. It integrates with the .NET 8 backend API to provide a user-friendly interface for managing PDF documents.

## Features

- User authentication (login/register)
- Document management (upload, view, delete)
- PDF viewing with pagination
- Document sharing with expiration dates
- Comment system with replies
- Responsive Material UI design

## Getting Started

### Prerequisites

- Node.js (v14 or later)
- npm or yarn
- The PDF Management .NET 8 backend running

### Installation

1. Navigate to the project directory:
   ```
   cd /mnt/d/pdf_management/frontend
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Start the development server:
   ```
   npm start
   ```

The application will be available at http://localhost:3000.

## Project Structure

- `/src/components/auth`: Authentication components (Login, Register)
- `/src/components/dashboard`: Dashboard and document list components
- `/src/components/documents`: PDF viewer and comment components
- `/src/components/layout`: Layout components (Header)
- `/src/components/common`: Shared components (ProtectedRoute)
- `/src/contexts`: React contexts (AuthContext)
- `/src/services`: API service functions

## Integration with Backend

The frontend is configured to proxy API requests to the backend running at https://localhost:7001. This is set in the `package.json` file.

## Technologies Used

- React
- React Router
- Material UI
- Axios for API requests
- react-pdf for PDF rendering
- JWT for authentication
