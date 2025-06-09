# PDF Management Frontend

A React-based frontend application for the PDF Management system. This application provides a user-friendly interface for managing PDF documents, with features for authentication, document management, sharing, and collaboration.

## Features

- **User Authentication**
  - Email/password registration and login
  - OAuth integration with Google
  - JWT-based authentication
  - Protected routes for authenticated users

- **Document Management**
  - Upload PDF documents with drag-and-drop support
  - View documents with pagination
  - Download and delete documents
  - Document organization and search

- **Document Sharing**
  - Generate secure sharing links with expiration dates
  - Configure sharing permissions (view, download, comment)
  - Access shared documents via unique tokens

- **Collaboration**
  - Add comments to specific pages and positions in PDFs
  - Reply to existing comments
  - View all comments for a document
  - Real-time updates for collaborative editing

- **User Settings**
  - Configure storage provider preferences
  - Manage Google Drive integration
  - View and update account information

- **Responsive Design**
  - Mobile-friendly interface
  - Adaptive layout for different screen sizes
  - Accessible UI components

## Technology Stack

- **Framework**: React 18
- **State Management**: React Context API
- **Routing**: React Router v6
- **UI Components**: 
  - Material UI components
  - Bootstrap for layout and styling
- **Form Handling**: 
  - Formik for form state management
  - Yup for form validation
- **HTTP Client**: Axios for API requests
- **PDF Rendering**: react-pdf library
- **Authentication**: JWT with OAuth 2.0
- **Date Handling**: date-fns for date formatting and manipulation
- **Development Tools**:
  - Create React App for project setup
  - ESLint for code quality
  - Prettier for code formatting

## Project Structure

```
/PDF_Management_frontend
├── /public                  # Static assets
│   ├── index.html           # HTML template
│   └── favicon.ico          # Site favicon
│
├── /src                     # Source code
│   ├── /components          # Reusable UI components
│   │   ├── /auth            # Authentication components
│   │   ├── /common          # Shared components
│   │   ├── /dashboard       # Dashboard components
│   │   ├── /documents       # Document-related components
│   │   └── /layout          # Layout components
│   │
│   ├── /contexts            # React contexts
│   │   └── AuthContext.js   # Authentication context
│   │
│   ├── /pages               # Page components
│   │   ├── DashboardPage.jsx
│   │   ├── LoginPage.jsx
│   │   ├── PdfViewerPage.jsx
│   │   ├── RegisterPage.jsx
│   │   ├── SettingsPage.jsx
│   │   └── UploadPage.jsx
│   │
│   ├── /services            # API service functions
│   │   └── api.js           # Axios configuration and API calls
│   │
│   ├── App.jsx              # Main application component
│   ├── index.js             # Application entry point
│   └── index.css            # Global styles
│
├── .env.example             # Environment variables template
├── package.json             # NPM dependencies and scripts
├── vercel.json              # Vercel deployment configuration
└── README.md                # Project documentation
```

## Getting Started

### Prerequisites

- Node.js (v14 or later)
- npm or yarn
- Backend API running (default: http://localhost:5000/api)

### Installation

1. Clone the repository and navigate to the project directory:
   ```
   cd PDF_Management_frontend
   ```

2. Install dependencies:
   ```
   npm install
   ```

3. Configure environment variables:
   - Copy `.env.example` to `.env`
   ```
   cp .env.example .env
   ```
   - Update the `.env` file with your settings:
   ```
   REACT_APP_API_URL=http://localhost:5000/api
   REACT_APP_AUTH_TOKEN_KEY=pdf_management_auth_token
   HTTPS=false
   ```

4. Start the development server:
   ```
   npm start
   ```

The application will be available at http://localhost:3000.

## Available Scripts

- `npm start` - Starts the development server
- `npm run start-win` - Starts the development server on Windows
- `npm run start-https` - Starts the development server with HTTPS
- `npm run build` - Creates a production build
- `npm test` - Runs the test suite
- `npm run eject` - Ejects from Create React App

## Routing

The application uses React Router for navigation with the following routes:

- `/login` - User login page
- `/register` - User registration page
- `/oauth-success` - OAuth callback handling
- `/` - Dashboard (protected)
- `/upload` - Document upload page (protected)
- `/view/:id` - PDF viewer page (protected)
- `/settings` - User settings page (protected)

## Authentication Flow

The application supports two authentication methods:

1. **JWT-based Authentication**:
   - User enters credentials on the login page
   - Backend validates credentials and returns a JWT token
   - Token is stored in localStorage
   - AuthContext provides authentication state to the application
   - Protected routes check authentication status via AuthContext

2. **OAuth with Google**:
   - User clicks "Sign in with Google" button
   - Google OAuth flow is initiated
   - After successful authentication, user is redirected to `/oauth-success`
   - Backend exchanges the authorization code for tokens
   - JWT token is stored and authentication proceeds as normal

## API Integration

The frontend connects to a RESTful API backend with the following features:

- **Base Configuration**:
  - API URL configured via environment variables
  - Axios instance with default configuration

- **Request Interceptors**:
  - Automatic JWT token inclusion in request headers
  - Error handling for network issues

- **Response Interceptors**:
  - Global error handling
  - Authentication error handling (token expiration)

- **API Services**:
  - Authentication (login, register, OAuth)
  - Document management (list, upload, download, delete)
  - Comments (add, list, update, delete)
  - Settings (get, update)

## Deployment

### Vercel Deployment

The project is configured for deployment on Vercel with the following settings in `vercel.json`:

```json
{
  "rewrites": [
    { "source": "/(.*)", "destination": "/index.html" }
  ],
  "buildCommand": "npm run build",
  "outputDirectory": "build",
  "framework": "create-react-app",
  "headers": [
    {
      "source": "/(.*)",
      "headers": [
        {
          "key": "Cache-Control",
          "value": "public, max-age=0, must-revalidate"
        },
        {
          "key": "X-Content-Type-Options",
          "value": "nosniff"
        },
        {
          "key": "X-Frame-Options",
          "value": "DENY"
        },
        {
          "key": "X-XSS-Protection",
          "value": "1; mode=block"
        }
      ]
    },
    {
      "source": "/static/(.*)",
      "headers": [
        {
          "key": "Cache-Control",
          "value": "public, max-age=31536000, immutable"
        }
      ]
    }
  ]
}
```

To deploy to Vercel:

1. Connect your GitHub repository to Vercel
2. Configure environment variables in the Vercel dashboard
3. Deploy with the default settings

## Component Highlights

### Authentication Components

- **LoginForm**: Email/password authentication with validation
- **RegisterForm**: User registration with field validation
- **GoogleOAuthButton**: Initiates Google OAuth flow
- **ProtectedRoute**: Route wrapper that redirects unauthenticated users

### Document Management Components

- **DocumentList**: Displays user's documents with actions
- **DocumentUploader**: Drag-and-drop file upload with progress
- **PdfViewer**: PDF rendering with pagination and zoom
- **ShareDialog**: Configure and generate sharing links

### Comment System Components

- **CommentList**: Displays comments for a document
- **CommentForm**: Add new comments to documents
- **CommentMarker**: Visual indicator of comment position on PDF

### Layout Components

- **NavBar**: Main navigation with authentication status
- **ErrorBoundary**: Catches and displays component errors

## State Management

The application uses React Context API for global state management:

- **AuthContext**: Manages authentication state and user information
  - `isAuthenticated`: Boolean indicating authentication status
  - `user`: Current user information
  - `login()`: Authenticates user and stores token
  - `logout()`: Removes authentication token and user data
  - `register()`: Creates new user account
  - `googleLogin()`: Handles Google OAuth authentication

## Error Handling

- **API Errors**: Handled through Axios interceptors
- **Component Errors**: Caught by ErrorBoundary component
- **Form Validation**: Implemented with Yup schemas
- **Authentication Errors**: Redirects to login page

## Security Considerations

- JWT tokens stored in localStorage
- Protected routes for authenticated content
- HTTPS recommended for production
- Security headers configured in Vercel deployment
- Input validation for all forms
- XSS protection through React's built-in protections

## Browser Compatibility

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)
- Mobile browsers (iOS Safari, Android Chrome)

## Contributing

1. Create a feature branch from `main`
2. Make your changes
3. Test your changes thoroughly
4. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
