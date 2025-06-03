import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Box,
  Typography,
  Alert,
  IconButton,
  InputAdornment,
  Snackbar
} from '@mui/material';
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { ContentCopy as ContentCopyIcon } from '@mui/icons-material';
import { shareDocument, shareDocumentJwt } from '../../services/api';
import { addDays } from 'date-fns';

const ShareDocumentDialog = ({ open, onClose, documentId }) => {
  const [expiryOption, setExpiryOption] = useState('7days');
  const [customExpiry, setCustomExpiry] = useState(addDays(new Date(), 7));
  const [shareLink, setShareLink] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [copySuccess, setCopySuccess] = useState(false);
  const [shareType, setShareType] = useState('standard');

  const handleExpiryChange = (event) => {
    setExpiryOption(event.target.value);
  };

  const handleShare = async () => {
    setLoading(true);
    setError('');
    
    try {
      let expiresAt = null;
      
      // Calculate expiry date based on selected option
      switch (expiryOption) {
        case '1day':
          expiresAt = addDays(new Date(), 1);
          break;
        case '7days':
          expiresAt = addDays(new Date(), 7);
          break;
        case '30days':
          expiresAt = addDays(new Date(), 30);
          break;
        case 'custom':
          expiresAt = customExpiry;
          break;
        case 'never':
        default:
          expiresAt = null;
          break;
      }
      
      // Generate share link based on selected type
      let response;
      if (shareType === 'jwt') {
        response = await shareDocumentJwt(documentId, expiresAt);
        // Use the complete URL from the backend
        setShareLink(response.shareUrl);
      } else {
        response = await shareDocument(documentId, expiresAt);
        // Make sure this is also a complete URL
        setShareLink(response.url);
      }
    } catch (err) {
      console.error('Error sharing document:', err);
      setError('Failed to generate share link. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const handleCopyLink = () => {
    navigator.clipboard.writeText(shareLink)
      .then(() => {
        setCopySuccess(true);
      })
      .catch(() => {
        setError('Failed to copy link to clipboard');
      });
  };

  const handleCloseSnackbar = () => {
    setCopySuccess(false);
  };

  const handleClose = () => {
    setShareLink('');
    setError('');
    onClose();
  };

  return (
    <>
      <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
        <DialogTitle>Share Document</DialogTitle>
        <DialogContent>
          {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
          
          {!shareLink ? (
            <>
              <FormControl fullWidth margin="normal">
                <InputLabel>Share Type</InputLabel>
                <Select
                  value={shareType}
                  onChange={(e) => setShareType(e.target.value)}
                  label="Share Type"
                >
                  <MenuItem value="standard">Standard Share</MenuItem>
                  <MenuItem value="jwt">JWT Token Share</MenuItem>
                </Select>
              </FormControl>
              
              <FormControl fullWidth margin="normal">
                <InputLabel>Link Expiration</InputLabel>
                <Select
                  value={expiryOption}
                  onChange={handleExpiryChange}
                  label="Link Expiration"
                >
                  <MenuItem value="1day">1 Day</MenuItem>
                  <MenuItem value="7days">7 Days</MenuItem>
                  <MenuItem value="30days">30 Days</MenuItem>
                  <MenuItem value="custom">Custom Date</MenuItem>
                  <MenuItem value="never">Never Expires</MenuItem>
                </Select>
              </FormControl>
              
              {expiryOption === 'custom' && (
                <Box mt={2}>
                  <LocalizationProvider dateAdapter={AdapterDateFns}>
                    <DateTimePicker
                      label="Custom Expiration Date"
                      value={customExpiry}
                      onChange={(newValue) => setCustomExpiry(newValue)}
                      minDateTime={new Date()}
                      slotProps={{ textField: { fullWidth: true } }}
                    />
                  </LocalizationProvider>
                </Box>
              )}
            </>
          ) : (
            <Box mt={2}>
              <Typography variant="subtitle1" gutterBottom>
                Share Link (expires: {expiryOption === 'never' 
                  ? 'Never' 
                  : new Date(
                      expiryOption === 'custom' 
                        ? customExpiry 
                        : addDays(
                            new Date(), 
                            expiryOption === '1day' ? 1 : expiryOption === '7days' ? 7 : 30
                          )
                    ).toLocaleString()})
              </Typography>
              
              <TextField
                fullWidth
                value={shareLink}
                InputProps={{
                  readOnly: true,
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton onClick={handleCopyLink} edge="end">
                        <ContentCopyIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
              
              <Typography variant="body2" color="textSecondary" sx={{ mt: 1 }}>
                Anyone with this link can view the document{shareType === 'jwt' ? ' using JWT authentication' : ''}.
              </Typography>
              
              <Alert severity="info" sx={{ mt: 2 }}>
                To test this link, copy it and paste it in a different browser or incognito window.
              </Alert>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={handleClose}>
            {shareLink ? 'Close' : 'Cancel'}
          </Button>
          {!shareLink && (
            <Button 
              onClick={handleShare} 
              variant="contained" 
              color="primary"
              disabled={loading}
            >
              {loading ? 'Generating...' : 'Generate Link'}
            </Button>
          )}
        </DialogActions>
      </Dialog>
      
      <Snackbar
        open={copySuccess}
        autoHideDuration={3000}
        onClose={handleCloseSnackbar}
        message="Link copied to clipboard"
      />
    </>
  );
};

export default ShareDocumentDialog;
