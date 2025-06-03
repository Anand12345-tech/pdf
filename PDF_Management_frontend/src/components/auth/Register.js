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

const Register = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: ''
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  
  const { register } = useAuth();
  const navigate = useNavigate();

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    // Validation
    if (formData.password !== formData.confirmPassword) {
      return setError('Passwords do not match');
    }
    
    if (formData.password.length < 8) {
      return setError('Password must be at least 8 characters long');
    }
    
    try {
      setError('');
      setLoading(true);
      
      const success = await register({
        email: formData.email,
        password: formData.password,
        firstName: formData.firstName,
        lastName: formData.lastName
      });
      
      if (success) {
        navigate('/login', { state: { message: 'Registration successful! Please log in.' } });
      } else {
        setError('Failed to register. Please try again.');
      }
    } catch (err) {
      setError('Failed to register: ' + (err.message || 'Unknown error'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <Paper elevation={3} className="auth-form">
      <Typography variant="h5" component="h1" gutterBottom align="center">
        Register
      </Typography>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      
      <Box component="form" onSubmit={handleSubmit}>
        <TextField
          label="Email"
          type="email"
          name="email"
          fullWidth
          margin="normal"
          value={formData.email}
          onChange={handleChange}
          required
          className="auth-form-field"
        />
        
        <TextField
          label="First Name"
          type="text"
          name="firstName"
          fullWidth
          margin="normal"
          value={formData.firstName}
          onChange={handleChange}
          required
          className="auth-form-field"
        />
        
        <TextField
          label="Last Name"
          type="text"
          name="lastName"
          fullWidth
          margin="normal"
          value={formData.lastName}
          onChange={handleChange}
          required
          className="auth-form-field"
        />
        
        <TextField
          label="Password"
          type="password"
          name="password"
          fullWidth
          margin="normal"
          value={formData.password}
          onChange={handleChange}
          required
          helperText="Password must be at least 8 characters long"
          className="auth-form-field"
        />
        
        <TextField
          label="Confirm Password"
          type="password"
          name="confirmPassword"
          fullWidth
          margin="normal"
          value={formData.confirmPassword}
          onChange={handleChange}
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
          {loading ? 'Registering...' : 'Register'}
        </Button>
        
        <Typography align="center">
          Already have an account? <Link component={RouterLink} to="/login">Login</Link>
        </Typography>
      </Box>
    </Paper>
  );
};

export default Register;
