import React from 'react';
import { Box, Typography, Button, Paper } from '@mui/material';

class ErrorBoundary extends React.Component {
  constructor(props) {
    super(props);
    this.state = { 
      hasError: false,
      error: null,
      errorInfo: null
    };
  }

  static getDerivedStateFromError(error) {
    // Update state so the next render will show the fallback UI
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    // You can log the error to an error reporting service
    console.error("Uncaught error:", error, errorInfo);
    this.setState({
      error: error,
      errorInfo: errorInfo
    });
    
    // Optional: Send error to a logging service
    // logErrorToService(error, errorInfo);
  }

  handleReload = () => {
    window.location.reload();
  }

  handleGoHome = () => {
    window.location.href = '/';
  }

  render() {
    if (this.state.hasError) {
      // You can render any custom fallback UI
      return (
        <Box 
          display="flex" 
          justifyContent="center" 
          alignItems="center" 
          minHeight="100vh"
          p={3}
        >
          <Paper 
            elevation={3} 
            sx={{ 
              p: 4, 
              maxWidth: 600, 
              textAlign: 'center' 
            }}
          >
            <Typography variant="h5" color="error" gutterBottom>
              Something went wrong
            </Typography>
            
            <Typography variant="body1" paragraph>
              We apologize for the inconvenience. The application has encountered an unexpected error.
            </Typography>
            
            {process.env.NODE_ENV === 'development' && this.state.error && (
              <Box 
                sx={{ 
                  mt: 2, 
                  p: 2, 
                  bgcolor: '#f5f5f5', 
                  borderRadius: 1,
                  textAlign: 'left',
                  overflow: 'auto',
                  maxHeight: '200px'
                }}
              >
                <Typography variant="body2" component="pre" sx={{ fontFamily: 'monospace' }}>
                  {this.state.error.toString()}
                </Typography>
                {this.state.errorInfo && (
                  <Typography variant="body2" component="pre" sx={{ fontFamily: 'monospace', mt: 2 }}>
                    {this.state.errorInfo.componentStack}
                  </Typography>
                )}
              </Box>
            )}
            
            <Box mt={4} display="flex" justifyContent="center" gap={2}>
              <Button 
                variant="contained" 
                color="primary" 
                onClick={this.handleReload}
              >
                Reload Page
              </Button>
              <Button 
                variant="outlined" 
                onClick={this.handleGoHome}
              >
                Go to Home
              </Button>
            </Box>
          </Paper>
        </Box>
      );
    }

    return this.props.children;
  }
}

export default ErrorBoundary;
