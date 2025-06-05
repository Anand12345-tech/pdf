import React, { useState } from 'react';
import { Button, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';

const GoogleOAuthButton = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  const handleGoogleAuth = () => {
    setIsLoading(true);
    setError(null);
    
    // Redirect to the backend OAuth endpoint
    window.location.href = 'http://localhost:5000/api/auth/google/login';
  };

  return (
    <div className="mt-3">
      {error && <Alert variant="danger">{error}</Alert>}
      <Button 
        variant="outline-primary" 
        onClick={handleGoogleAuth} 
        disabled={isLoading}
        className="d-flex align-items-center justify-content-center w-100"
      >
        <img 
          src="https://upload.wikimedia.org/wikipedia/commons/5/53/Google_%22G%22_Logo.svg" 
          alt="Google logo" 
          style={{ width: '20px', marginRight: '10px' }} 
        />
        {isLoading ? 'Connecting...' : 'Connect with Google Drive'}
      </Button>
      <small className="text-muted mt-2 d-block">
        Connect your Google Drive account to store PDFs in the cloud
      </small>
    </div>
  );
};

export default GoogleOAuthButton;
