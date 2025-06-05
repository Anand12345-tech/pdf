import React, { useState, useEffect } from 'react';
import { Container, Card, Form, Button, Alert } from 'react-bootstrap';
import GoogleOAuthButton from '../components/GoogleOAuthButton';
import { useAuth } from '../contexts/AuthContext';
import api from '../services/api';

const SettingsPage = () => {
  const { user } = useAuth();
  const [isGoogleConnected, setIsGoogleConnected] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [storageProvider, setStorageProvider] = useState('GoogleDrive');
  const [saveSuccess, setSaveSuccess] = useState(false);

  useEffect(() => {
    // Check if Google Drive is connected
    const checkGoogleConnection = async () => {
      try {
        setLoading(true);
        const response = await api.get('/api/settings/storage-status');
        setIsGoogleConnected(response.data.isGoogleDriveConnected);
        setStorageProvider(response.data.currentProvider || 'GoogleDrive');
      } catch (err) {
        console.error('Error checking Google Drive connection:', err);
        setError('Failed to check storage connection status');
      } finally {
        setLoading(false);
      }
    };

    checkGoogleConnection();
  }, []);

  const handleSaveSettings = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      setSaveSuccess(false);
      
      await api.post('/api/settings/storage', { provider: storageProvider });
      
      setSaveSuccess(true);
      setTimeout(() => setSaveSuccess(false), 3000);
    } catch (err) {
      console.error('Error saving settings:', err);
      setError('Failed to save settings');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container className="py-5">
      <h1 className="mb-4">Settings</h1>
      
      {error && <Alert variant="danger">{error}</Alert>}
      {saveSuccess && <Alert variant="success">Settings saved successfully!</Alert>}
      
      <Card className="mb-4">
        <Card.Header as="h5">Storage Settings</Card.Header>
        <Card.Body>
          <Form onSubmit={handleSaveSettings}>
            <Form.Group className="mb-3">
              <Form.Label>Storage Provider</Form.Label>
              <Form.Select 
                value={storageProvider} 
                onChange={(e) => setStorageProvider(e.target.value)}
                disabled={loading}
              >
                <option value="Local">Local Storage</option>
                <option value="GoogleDrive">Google Drive</option>
              </Form.Select>
              <Form.Text className="text-muted">
                Choose where to store your PDF files
              </Form.Text>
            </Form.Group>
            
            {storageProvider === 'GoogleDrive' && (
              <div className="mb-3">
                <div className="d-flex align-items-center mb-2">
                  <h6 className="mb-0 me-2">Google Drive Connection</h6>
                  {isGoogleConnected ? (
                    <span className="badge bg-success">Connected</span>
                  ) : (
                    <span className="badge bg-warning text-dark">Not Connected</span>
                  )}
                </div>
                
                {!isGoogleConnected && <GoogleOAuthButton />}
                
                {isGoogleConnected && (
                  <Button 
                    variant="outline-danger" 
                    size="sm"
                    onClick={async () => {
                      try {
                        await api.post('/api/settings/disconnect-google');
                        setIsGoogleConnected(false);
                      } catch (err) {
                        setError('Failed to disconnect Google Drive');
                      }
                    }}
                  >
                    Disconnect Google Drive
                  </Button>
                )}
              </div>
            )}
            
            <Button type="submit" variant="primary" disabled={loading}>
              {loading ? 'Saving...' : 'Save Settings'}
            </Button>
          </Form>
        </Card.Body>
      </Card>
      
      <Card>
        <Card.Header as="h5">Account Information</Card.Header>
        <Card.Body>
          <p><strong>Email:</strong> {user?.email}</p>
          <p><strong>Name:</strong> {user?.firstName} {user?.lastName}</p>
        </Card.Body>
      </Card>
    </Container>
  );
};

export default SettingsPage;
