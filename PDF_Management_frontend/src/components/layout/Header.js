import React from 'react';
import { AppBar, Toolbar, Typography, Button, Box } from '@mui/material';
import { Link as RouterLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf';

const Header = () => {
  const { isAuthenticated, logout, currentUser } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <PictureAsPdfIcon sx={{ mr: 2 }} />
        <Typography variant="h6" component={RouterLink} to="/" sx={{ flexGrow: 1, textDecoration: 'none', color: 'white' }}>
          PDF Management
        </Typography>
        
        {isAuthenticated ? (
          <Box>
            <Typography variant="body1" component="span" sx={{ mr: 2 }}>
              {currentUser?.email}
            </Typography>
            <Button color="inherit" onClick={handleLogout}>Logout</Button>
          </Box>
        ) : (
          <Box>
            <Button color="inherit" component={RouterLink} to="/login">Login</Button>
            <Button color="inherit" component={RouterLink} to="/register">Register</Button>
          </Box>
        )}
      </Toolbar>
    </AppBar>
  );
};

export default Header;
