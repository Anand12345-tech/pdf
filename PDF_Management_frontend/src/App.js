import React from 'react';
import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Container } from '@mui/material';

import Header from './components/layout/Header';
import Login from './components/auth/Login';
import Register from './components/auth/Register';
import Dashboard from './components/dashboard/Dashboard';
import DocumentView from './components/documents/DocumentView';
import SharedPdfViewer from './components/documents/SharedPdfViewer';
import ProtectedRoute from './components/common/ProtectedRoute';
import ErrorBoundary from './components/ErrorBoundary';
import { useAuth } from './contexts/AuthContext';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function App() {
  const { isAuthenticated } = useAuth();
  const location = useLocation();
  
  // Check if the current route is a shared PDF view
  const isSharedPdfRoute = location.pathname.includes('/shared-pdf/') || 
                          location.pathname.includes('/api/Public/view/');

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      {/* Only show header if not on a shared PDF route */}
      {!isSharedPdfRoute && <Header />}
      <Container sx={{ mt: isSharedPdfRoute ? 0 : 4, mb: 4, p: isSharedPdfRoute ? 0 : undefined, maxWidth: isSharedPdfRoute ? 'none' : undefined }}>
        <ErrorBoundary>
          <Routes>
          <Route path="/login" element={!isAuthenticated ? <Login /> : <Navigate to="/dashboard" />} />
          <Route path="/register" element={!isAuthenticated ? <Register /> : <Navigate to="/dashboard" />} />
          <Route path="/dashboard" element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          } />
          <Route path="/documents/:id" element={
            <ProtectedRoute>
              <DocumentView />
            </ProtectedRoute>
          } />
          {/* Public routes for shared PDFs - No header for shared views */}
          <Route path="/shared-pdf/:token" element={
            <>
              <SharedPdfViewer />
            </>
          } />
          {/* Legacy format support */}
          <Route path="/api/Public/view/:token" element={<Navigate to={location => `/shared-pdf/${location.pathname.split('/').pop()}`} />} />
          <Route path="/" element={<Navigate to={isAuthenticated ? "/dashboard" : "/login"} />} />
        </Routes>
        </ErrorBoundary>
      </Container>
    </ThemeProvider>
  );
}

export default App;
