import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  CardActions,
  Typography,
  Button,
  IconButton,
  Menu,
  MenuItem,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  TextField,
  Box,
  Snackbar,
  Alert
} from '@mui/material';
import { 
  MoreVert as MoreVertIcon,
  Share as ShareIcon,
  Delete as DeleteIcon,
  FileCopy as FileCopyIcon
} from '@mui/icons-material';
import { deleteDocument, shareDocument } from '../../services/api';
import { format } from 'date-fns';

const DocumentCard = ({ document, onDeleted }) => {
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = useState(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [shareDialogOpen, setShareDialogOpen] = useState(false);
  const [expiryDate, setExpiryDate] = useState(
    format(new Date(Date.now() + 7 * 24 * 60 * 60 * 1000), 'yyyy-MM-dd')
  );
  const [shareLink, setShareLink] = useState('');
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState('');
  const [snackbarSeverity, setSnackbarSeverity] = useState('success');

  const handleMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleViewDocument = () => {
    navigate(`/documents/${document.id}`);
  };

  const handleDeleteClick = () => {
    handleMenuClose();
    setDeleteDialogOpen(true);
  };

  const handleShareClick = () => {
    handleMenuClose();
    setShareDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    try {
      await deleteDocument(document.id);
      setDeleteDialogOpen(false);
      if (onDeleted) onDeleted();
      showSnackbar('Document deleted successfully', 'success');
    } catch (error) {
      console.error('Error deleting document:', error);
      showSnackbar('Failed to delete document', 'error');
    }
  };

  const handleShareConfirm = async () => {
    try {
      const response = await shareDocument(document.id, expiryDate);
      setShareLink(response.url);
      showSnackbar('Document shared successfully', 'success');
    } catch (error) {
      console.error('Error sharing document:', error);
      showSnackbar('Failed to share document', 'error');
    }
  };

  const handleCopyLink = () => {
    navigator.clipboard.writeText(shareLink);
    showSnackbar('Link copied to clipboard', 'success');
  };

  const showSnackbar = (message, severity) => {
    setSnackbarMessage(message);
    setSnackbarSeverity(severity);
    setSnackbarOpen(true);
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return format(date, 'MMM d, yyyy h:mm a');
  };

  const formatFileSize = (bytes) => {
    if (bytes < 1024) return bytes + ' B';
    else if (bytes < 1048576) return (bytes / 1024).toFixed(1) + ' KB';
    else return (bytes / 1048576).toFixed(1) + ' MB';
  };

  return (
    <>
      <Card className="document-card">
        <CardContent>
          <Box display="flex" justifyContent="space-between" alignItems="flex-start">
            <Typography variant="h6" component="h2" noWrap sx={{ maxWidth: '80%' }}>
              {document.fileName}
            </Typography>
            <IconButton size="small" onClick={handleMenuOpen}>
              <MoreVertIcon />
            </IconButton>
          </Box>
          <Typography variant="body2" color="textSecondary" gutterBottom>
            Uploaded: {formatDate(document.uploadedAt)}
          </Typography>
          <Typography variant="body2" color="textSecondary">
            Size: {formatFileSize(document.fileSize)}
          </Typography>
        </CardContent>
        <CardActions>
          <Button size="small" color="primary" onClick={handleViewDocument}>
            View Document
          </Button>
        </CardActions>
      </Card>

      {/* Menu */}
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={handleShareClick}>
          <ShareIcon fontSize="small" sx={{ mr: 1 }} />
          Share
        </MenuItem>
        <MenuItem onClick={handleDeleteClick}>
          <DeleteIcon fontSize="small" sx={{ mr: 1 }} />
          Delete
        </MenuItem>
      </Menu>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
      >
        <DialogTitle>Delete Document</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete "{document.fileName}"? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error">Delete</Button>
        </DialogActions>
      </Dialog>

      {/* Share Dialog */}
      <Dialog
        open={shareDialogOpen}
        onClose={() => {
          setShareDialogOpen(false);
          setShareLink('');
        }}
      >
        <DialogTitle>Share Document</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Set an expiration date for the shared link:
          </DialogContentText>
          <TextField
            type="date"
            fullWidth
            margin="normal"
            value={expiryDate}
            onChange={(e) => setExpiryDate(e.target.value)}
          />
          {shareLink && (
            <Box mt={2}>
              <Typography variant="subtitle2">Share Link:</Typography>
              <Box display="flex" alignItems="center" mt={1}>
                <TextField
                  fullWidth
                  value={shareLink}
                  InputProps={{ readOnly: true }}
                  size="small"
                />
                <IconButton onClick={handleCopyLink} size="small" sx={{ ml: 1 }}>
                  <FileCopyIcon fontSize="small" />
                </IconButton>
              </Box>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShareDialogOpen(false)}>Close</Button>
          {!shareLink && (
            <Button onClick={handleShareConfirm} color="primary">
              Generate Link
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* Snackbar for notifications */}
      <Snackbar
        open={snackbarOpen}
        autoHideDuration={6000}
        onClose={() => setSnackbarOpen(false)}
      >
        <Alert 
          onClose={() => setSnackbarOpen(false)} 
          severity={snackbarSeverity}
          sx={{ width: '100%' }}
        >
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </>
  );
};

export default DocumentCard;
