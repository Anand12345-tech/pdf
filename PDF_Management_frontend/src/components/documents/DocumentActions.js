import React, { useState } from 'react';
import { 
  Box, 
  Button, 
  IconButton, 
  Menu, 
  MenuItem, 
  ListItemIcon, 
  ListItemText,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions
} from '@mui/material';
import {
  MoreVert as MoreVertIcon,
  Delete as DeleteIcon,
  Share as ShareIcon,
  Download as DownloadIcon
} from '@mui/icons-material';
import { deleteDocument } from '../../services/api';
import ShareDocumentDialog from './ShareDocumentDialog';

const DocumentActions = ({ document, onDocumentDeleted }) => {
  const [anchorEl, setAnchorEl] = useState(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [shareDialogOpen, setShareDialogOpen] = useState(false);
  const [loading, setLoading] = useState(false);

  const handleMenuOpen = (event) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleShareClick = () => {
    handleMenuClose();
    setShareDialogOpen(true);
  };

  const handleDeleteClick = () => {
    handleMenuClose();
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    setLoading(true);
    try {
      await deleteDocument(document.id);
      setDeleteDialogOpen(false);
      if (onDocumentDeleted) {
        onDocumentDeleted(document.id);
      }
    } catch (error) {
      console.error('Error deleting document:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <Box>
        <IconButton
          aria-label="document actions"
          aria-controls="document-menu"
          aria-haspopup="true"
          onClick={handleMenuOpen}
        >
          <MoreVertIcon />
        </IconButton>
        <Menu
          id="document-menu"
          anchorEl={anchorEl}
          keepMounted
          open={Boolean(anchorEl)}
          onClose={handleMenuClose}
        >
          <MenuItem onClick={handleShareClick}>
            <ListItemIcon>
              <ShareIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Share" />
          </MenuItem>
          <MenuItem component="a" href={document.downloadUrl} download>
            <ListItemIcon>
              <DownloadIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Download" />
          </MenuItem>
          <MenuItem onClick={handleDeleteClick}>
            <ListItemIcon>
              <DeleteIcon fontSize="small" color="error" />
            </ListItemIcon>
            <ListItemText primary="Delete" sx={{ color: 'error.main' }} />
          </MenuItem>
        </Menu>
      </Box>

      {/* Share Dialog */}
      <ShareDocumentDialog
        open={shareDialogOpen}
        onClose={() => setShareDialogOpen(false)}
        documentId={document.id}
      />

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
          <Button onClick={() => setDeleteDialogOpen(false)} disabled={loading}>
            Cancel
          </Button>
          <Button 
            onClick={handleDeleteConfirm} 
            color="error" 
            disabled={loading}
            autoFocus
          >
            {loading ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default DocumentActions;
