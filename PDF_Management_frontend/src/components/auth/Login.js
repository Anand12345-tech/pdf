import React, { useState } from 'react';
import { useNavigate, Link as RouterLink } from 'react-router-dom';
import { 
  TextField, 
  Button, 
  Typography, 
  Paper, 
  Box, 
  Alert,
  Link
} from '@mui/material';
import { useAuth } from '../../contexts/AuthContext';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!email || !password) {
      setError('Please enter both email and password');
      return;
    }
    
    try {
      setError('');
      setLoading(true);
      
      const success = await login(email, password);
      
      if (success) {
        navigate('/dashboard');
      } else {
        setError('Failed to log in. Please check your credentials.');
      }
    } catch (err) {
      setError('Failed to log in: ' + (err.message || 'Unknown error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Paper elevation={3} className="auth-form">
      <Typography variant="h5" component="h1" gutterBottom align="center">
        Login
      </Typography>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      
      <Box component="form" onSubmit={handleSubmit}>
        <TextField
          label="Email"
          type="email"
          fullWidth
          margin="normal"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          className="auth-form-field"
        />
        
        <TextField
          label="Password"
          type="password"
          fullWidth
          margin="normal"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          className="auth-form-field"
        />
        
        <Button
          type="submit"
          variant="contained"
          color="primary"
          fullWidth
          disabled={loading}
          sx={{ mt: 2, mb: 2 }}
        >
          {loading ? 'Logging in...' : 'Login'}
        </Button>
        
        <Typography align="center">
          Don't have an account? <Link component={RouterLink} to="/register">Register</Link>
        </Typography>
      </Box>
    </Paper>
  );
};

export default Login;
