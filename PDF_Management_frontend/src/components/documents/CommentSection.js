import React, { useState, useEffect } from 'react';
import {
  Box,
  Typography,
  TextField,
  Button,
  Card,
  CardContent,
  CardActions,
  IconButton,
  Menu,
  MenuItem,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Divider,
  Alert
} from '@mui/material';
import {
  MoreVert as MoreVertIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Reply as ReplyIcon
} from '@mui/icons-material';
import { addComment, updateComment, deleteComment } from '../../services/api';
import { format } from 'date-fns';
import { useAuth } from '../../contexts/AuthContext';

const CommentSection = ({ 
  documentId, 
  comments, 
  currentPage, 
  onCommentAdded, 
  onCommentUpdated, 
  onCommentDeleted 
}) => {
  const { currentUser } = useAuth();
  const [newComment, setNewComment] = useState('');
  const [replyTo, setReplyTo] = useState(null);
  const [editingComment, setEditingComment] = useState(null);
  const [editText, setEditText] = useState('');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [commentToDelete, setCommentToDelete] = useState(null);
  const [anchorEl, setAnchorEl] = useState(null);
  const [selectedComment, setSelectedComment] = useState(null);
  const [error, setError] = useState('');

  // Load stored commenter names on component mount
  useEffect(() => {
    // Try to load any stored comment names from localStorage
    try {
      const storedNamesStr = localStorage.getItem('commentNames');
      if (storedNamesStr) {
        const storedNames = JSON.parse(storedNamesStr);
        
        // Update comments with stored names if they don't already have names
        const updatedComments = comments.map(comment => {
          if (!comment.commenterName && storedNames[comment.id]) {
            return {
              ...comment,
              commenterName: storedNames[comment.id]
            };
          }
          
          // Also check replies
          if (comment.replies && comment.replies.length > 0) {
            const updatedReplies = comment.replies.map(reply => {
              if (!reply.commenterName && storedNames[reply.id]) {
                return {
                  ...reply,
                  commenterName: storedNames[reply.id]
                };
              }
              return reply;
            });
            
            return {
              ...comment,
              replies: updatedReplies
            };
          }
          
          return comment;
        });
        
        // If we have any updates, notify parent component
        const hasUpdates = JSON.stringify(updatedComments) !== JSON.stringify(comments);
        if (hasUpdates && onCommentUpdated) {
          updatedComments.forEach(comment => {
            const originalComment = comments.find(c => c.id === comment.id);
            if (JSON.stringify(comment) !== JSON.stringify(originalComment)) {
              onCommentUpdated(comment);
            }
            
            // Check replies too
            if (comment.replies && comment.replies.length > 0) {
              comment.replies.forEach(reply => {
                const originalReply = originalComment?.replies?.find(r => r.id === reply.id);
                if (JSON.stringify(reply) !== JSON.stringify(originalReply)) {
                  onCommentUpdated(reply);
                }
              });
            }
          });
        }
      }
    } catch (e) {
      console.error('Error processing stored comment names:', e);
    }
  }, [comments, onCommentUpdated]);

  // Filter comments for the current page
  const pageComments = comments.filter(comment => 
    comment.pageNumber === currentPage && !comment.parentCommentId
  );

  const handleMenuOpen = (event, comment) => {
    setAnchorEl(event.currentTarget);
    setSelectedComment(comment);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
    setSelectedComment(null);
  };

 const handleAddComment = async () => {
  if (!newComment.trim()) return;

  try {
    const response = await addComment(
      documentId,
      newComment,
      currentPage,
      replyTo?.id
    );

    if (onCommentAdded) onCommentAdded(response);
    setNewComment('');
    setReplyTo(null);
    setError('');
  } catch (err) {
    console.error('Error adding comment:', err);
    setError('Failed to add comment. Please try again.');
  }
};

  const handleReplyClick = (comment) => {
    handleMenuClose();
    setReplyTo({
      ...comment,
      // Ensure we use the best available name for the comment
      commenterName: comment.commenterName || 
                    (() => {
                      try {
                        const storedNames = JSON.parse(localStorage.getItem('commentNames') || '{}');
                        return storedNames[comment.id];
                      } catch (e) {
                        return null;
                      }
                    })() || 
                    (comment.userType === 'invited' ? 'Guest User' : 'Anonymous')
    });
  };

  const handleEditClick = (comment) => {
    handleMenuClose();
    setEditingComment(comment);
    setEditText(comment.content);
  };

  const handleDeleteClick = (comment) => {
    handleMenuClose();
    setCommentToDelete(comment);
    setDeleteDialogOpen(true);
  };

  const handleSaveEdit = async () => {
    if (!editText.trim() || !editingComment) return;

    try {
      const response = await updateComment(editingComment.id, editText);
      if (onCommentUpdated) onCommentUpdated(response);
      setEditingComment(null);
      setEditText('');
      setError('');
    } catch (err) {
      console.error('Error updating comment:', err);
      setError('Failed to update comment. Please try again.');
    }
  };

  const handleDeleteConfirm = async () => {
    if (!commentToDelete) return;

    try {
      await deleteComment(commentToDelete.id);
      if (onCommentDeleted) onCommentDeleted(commentToDelete.id);
      setDeleteDialogOpen(false);
      setCommentToDelete(null);
      setError('');
    } catch (err) {
      console.error('Error deleting comment:', err);
      setError('Failed to delete comment. Please try again.');
    }
  };

  const formatDate = (dateString) => {
    return format(new Date(dateString), 'MMM d, yyyy h:mm a');
  };

  const renderComment = (comment, isReply = false) => {
    const isCurrentUserComment = comment.commenterId === currentUser?.id;
    
    // Get stored names from localStorage if available
    let storedNames = {};
    try {
      const storedNamesStr = localStorage.getItem('commentNames');
      if (storedNamesStr) {
        storedNames = JSON.parse(storedNamesStr);
      }
    } catch (e) {
      console.error('Error parsing stored names:', e);
    }
    
    // Use stored name if available
    const displayName = comment.commenterName || 
                        storedNames[comment.id] || 
                        (comment.userType === 'invited' ? 'Guest User' : 'Anonymous');

    return (
      <Card key={comment.id} className={isReply ? "comment-reply" : "comment-card"} sx={{ mb: 2 }}>
        <CardContent>
          <Box display="flex" justifyContent="space-between" alignItems="center">
            <Typography variant="subtitle2" color="primary">
              {displayName}
            </Typography>
            {isCurrentUserComment && (
              <IconButton size="small" onClick={(e) => handleMenuOpen(e, comment)}>
                <MoreVertIcon fontSize="small" />
              </IconButton>
            )}
          </Box>
          
          {editingComment?.id === comment.id ? (
            <TextField
              fullWidth
              multiline
              value={editText}
              onChange={(e) => setEditText(e.target.value)}
              margin="normal"
              variant="outlined"
              size="small"
            />
          ) : (
            <Typography variant="body1" sx={{ mt: 1 }}>
              {comment.content}
            </Typography>
          )}
          
          <Typography variant="caption" color="textSecondary" display="block" sx={{ mt: 1 }}>
            {formatDate(comment.createdAt)}
          </Typography>
        </CardContent>
        
        {editingComment?.id === comment.id ? (
          <CardActions>
            <Button size="small" onClick={() => setEditingComment(null)}>Cancel</Button>
            <Button size="small" color="primary" onClick={handleSaveEdit}>Save</Button>
          </CardActions>
        ) : (
          <CardActions>
            <Button 
              size="small" 
              startIcon={<ReplyIcon />} 
              onClick={() => handleReplyClick(comment)}
            >
              Reply
            </Button>
          </CardActions>
        )}
        
        {/* Render replies */}
        {comment.replies && comment.replies.length > 0 && (
          <Box sx={{ pl: 2, pr: 2, pb: 1 }}>
            {comment.replies.map(reply => renderComment(reply, true))}
          </Box>
        )}
      </Card>
    );
  };

  return (
    <Box className="comment-section">
      <Typography variant="h6" gutterBottom>
        Comments {currentPage && `(Page ${currentPage})`}
      </Typography>
      
      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}
      
      <Box mb={3}>
        <TextField
          fullWidth
          multiline
          rows={3}
          placeholder={replyTo ? `Reply to ${replyTo.commenterName}...` : "Add a comment..."}
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          variant="outlined"
        />
        <Box display="flex" justifyContent="space-between" alignItems="center" mt={1}>
          {replyTo && (
            <Typography variant="body2" color="textSecondary">
              Replying to {replyTo.commenterName}
              <Button size="small" onClick={() => setReplyTo(null)} sx={{ ml: 1 }}>
                Cancel
              </Button>
            </Typography>
          )}
          <Button 
            variant="contained" 
            color="primary" 
            onClick={handleAddComment}
            disabled={!newComment.trim()}
            sx={{ ml: 'auto' }}
          >
            {replyTo ? 'Reply' : 'Comment'}
          </Button>
        </Box>
      </Box>
      
      <Divider sx={{ mb: 3 }} />
      
      {pageComments.length === 0 ? (
        <Typography variant="body2" color="textSecondary" align="center">
          No comments on this page yet. Be the first to comment!
        </Typography>
      ) : (
        pageComments.map(comment => renderComment(comment))
      )}
      
      {/* Comment Menu */}
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={() => handleEditClick(selectedComment)}>
          <EditIcon fontSize="small" sx={{ mr: 1 }} />
          Edit
        </MenuItem>
        <MenuItem onClick={() => handleDeleteClick(selectedComment)}>
          <DeleteIcon fontSize="small" sx={{ mr: 1 }} />
          Delete
        </MenuItem>
      </Menu>
      
      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
      >
        <DialogTitle>Delete Comment</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete this comment? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error">Delete</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CommentSection;
