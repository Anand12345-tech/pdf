import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Box, 
  Typography, 
  Paper, 
  CircularProgress, 
  Button,
  Alert,
  Container,
  TextField,
  Divider,
  Card,
  CardContent,
  CardActions
} from '@mui/material';
import { Document, Page, pdfjs } from 'react-pdf';
import { viewSharedDocumentJwt, addPublicComment, getPublicComments } from '../../services/api';

// Set up the worker for react-pdf
pdfjs.GlobalWorkerOptions.workerSrc = `//cdnjs.cloudflare.com/ajax/libs/pdf.js/${pdfjs.version}/pdf.worker.min.js`;

const SharedPdfViewer = () => {
  const { token } = useParams();
  const navigate = useNavigate();
  const [pdfUrl, setPdfUrl] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [numPages, setNumPages] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pdfError, setPdfError] = useState(false);
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState('');
  const [commenterName, setCommenterName] = useState('');
  const [commentError, setCommentError] = useState('');
  const [documentInfo, setDocumentInfo] = useState(null);
  const [isUuid, setIsUuid] = useState(false);

  // Helper function to get API base URL
  const getApiBaseUrl = () => {
    return process.env.REACT_APP_API_URL || 
           `${window.location.protocol}//${window.location.host.replace('3000', '5000')}/api`;
  };

  useEffect(() => {
    const loadSharedDocument = async () => {
      try {
        setLoading(true);
        
        // Check if token is a UUID (standard share) or JWT token
        const uuidPattern = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
        const isUuidToken = uuidPattern.test(token);
        setIsUuid(isUuidToken);
        
        let url;
        if (isUuidToken) {
          // Handle standard token (UUID)
          // Get API base URL from environment or use default
          const apiBaseUrl = getApiBaseUrl();
          
          // Convert to API endpoint format for direct viewing
          const apiUrl = `${apiBaseUrl}/Public/view/${token}`;
          
          // Fetch document details first
          const response = await fetch(apiUrl);
          if (!response.ok) {
            throw new Error('Failed to fetch document');
          }
          const data = await response.json();
          setDocumentInfo(data.document);
          
          // Process comments to ensure commenterName is properly used
          const processedComments = data.comments?.map(comment => {
            // Try to get stored name from localStorage
            let storedNames = {};
            try {
              const storedNamesStr = localStorage.getItem('commentNames');
              if (storedNamesStr) {
                storedNames = JSON.parse(storedNamesStr);
              }
            } catch (e) {
              console.error('Error parsing stored names:', e);
            }
            
            return {
              ...comment,
              commenterName: comment.commenterName || storedNames[comment.id] || 'Anonymous',
              replies: comment.replies?.map(reply => ({
                ...reply,
                commenterName: reply.commenterName || storedNames[reply.id] || 'Anonymous'
              })) || []
            };
          }) || [];
          setComments(processedComments);
          
          // Then get the download URL for viewing
          const downloadUrl = `${apiBaseUrl}/Public/download/${token}`;
          const blobResponse = await fetch(downloadUrl);
          const blob = await blobResponse.blob();
          url = URL.createObjectURL(blob);
        } else {
          // Handle JWT token
          url = await viewSharedDocumentJwt(token);
          // For JWT tokens, we need to fetch comments separately
          try {
            // Extract the token ID from the JWT (if possible)
            const tokenParts = token.split('.');
            if (tokenParts.length === 3) {
              const payload = JSON.parse(atob(tokenParts[1]));
              if (payload.tokenId) {
                const commentsResponse = await fetch(`${getApiBaseUrl()}/Public/view/${payload.tokenId}`);
                if (commentsResponse.ok) {
                  const data = await commentsResponse.json();
                  setDocumentInfo(data.document);
                  
                  // Process comments to ensure commenterName is properly used
                  const processedComments = data.comments?.map(comment => {
                    // Try to get stored name from localStorage
                    let storedNames = {};
                    try {
                      const storedNamesStr = localStorage.getItem('commentNames');
                      if (storedNamesStr) {
                        storedNames = JSON.parse(storedNamesStr);
                      }
                    } catch (e) {
                      console.error('Error parsing stored names:', e);
                    }
                    
                    return {
                      ...comment,
                      commenterName: comment.commenterName || storedNames[comment.id] || 'Anonymous',
                      replies: comment.replies?.map(reply => ({
                        ...reply,
                        commenterName: reply.commenterName || storedNames[reply.id] || 'Anonymous'
                      })) || []
                    };
                  }) || [];
                  setComments(processedComments);
                }
              }
            }
          } catch (commentErr) {
            console.error('Error fetching comments:', commentErr);
            // Non-critical error, don't show to user
          }
        }
        
        setPdfUrl(url);
        setError('');
      } catch (err) {
        console.error('Error loading shared document:', err);
        setError('This document is not available. The link may have expired or been revoked.');
      } finally {
        setLoading(false);
      }
    };

    if (token) {
      loadSharedDocument();
    } else {
      setError('Invalid share link');
      setLoading(false);
    }
  }, [token]);

  const onDocumentLoadSuccess = ({ numPages }) => {
    setNumPages(numPages);
    setPdfError(false);
  };

  const onDocumentLoadError = () => {
    setPdfError(true);
  };

  const handlePreviousPage = () => {
    setPageNumber(prevPageNumber => Math.max(prevPageNumber - 1, 1));
  };

  const handleNextPage = () => {
    setPageNumber(prevPageNumber => Math.min(prevPageNumber + 1, numPages));
  };

  const handleAddComment = async () => {
    if (!newComment.trim()) return;
    if (!commenterName.trim()) {
      setCommentError('Please provide your name');
      return;
    }
    
    try {
      setCommentError('');
      
      // Store the commenter name in localStorage for future use
      localStorage.setItem('commenterName', commenterName);
      
      // Include commenter name in the request
      const response = await addPublicComment(token, newComment, pageNumber, null, commenterName);
      
      // Add the new comment to the list with the commenter name explicitly set
      setComments(prevComments => [
        ...prevComments, 
        {
          ...response,
          commenterName: commenterName // Explicitly set the commenter name
        }
      ]);
      setNewComment('');
      
      // Refresh comments to ensure we have the latest data
      setTimeout(() => refreshComments(), 500);
    } catch (err) {
      console.error('Error adding comment:', err);
      setCommentError('Failed to add comment. Please try again.');
    }
  };

  // Refresh comments to ensure we have the latest data with proper names
  const refreshComments = async () => {
    try {
      if (!token) return;
      
      // For UUID tokens, we can fetch comments directly
      if (isUuid) {
        const apiUrl = `${getApiBaseUrl()}/Public/view/${token}`;
        const response = await fetch(apiUrl);
        if (response.ok) {
          const data = await response.json();
          
          // Process comments to ensure commenterName is properly used
          const processedComments = data.comments?.map(comment => {
            // Check if the comment has a stored commenterName in the response
            // If not, use the one we have in our local state if available
            const existingComment = comments.find(c => c.id === comment.id);
            
            // Try to get stored name from localStorage
            let storedNames = {};
            try {
              const storedNamesStr = localStorage.getItem('commentNames');
              if (storedNamesStr) {
                storedNames = JSON.parse(storedNamesStr);
              }
            } catch (e) {
              console.error('Error parsing stored names:', e);
            }
            
            return {
              ...comment,
              commenterName: comment.commenterName || storedNames[comment.id] || existingComment?.commenterName || 'Anonymous',
              replies: comment.replies?.map(reply => {
                const existingReply = existingComment?.replies?.find(r => r.id === reply.id);
                return {
                  ...reply,
                  commenterName: reply.commenterName || storedNames[reply.id] || existingReply?.commenterName || 'Anonymous'
                };
              }) || []
            };
          }) || [];
          
          setComments(processedComments);
        }
      } else {
        // For JWT tokens, extract the token ID if possible
        try {
          const tokenParts = token.split('.');
          if (tokenParts.length === 3) {
            const payload = JSON.parse(atob(tokenParts[1]));
            if (payload.tokenId) {
              const commentsResponse = await fetch(`${getApiBaseUrl()}/Public/view/${payload.tokenId}`);
              if (commentsResponse.ok) {
                const data = await commentsResponse.json();
                
                // Process comments to ensure commenterName is properly used
                const processedComments = data.comments?.map(comment => {
                  // Check if the comment has a stored commenterName in the response
                  // If not, use the one we have in our local state if available
                  const existingComment = comments.find(c => c.id === comment.id);
                  return {
                    ...comment,
                    commenterName: comment.commenterName || existingComment?.commenterName || 'Anonymous',
                    replies: comment.replies?.map(reply => {
                      const existingReply = existingComment?.replies?.find(r => r.id === reply.id);
                      return {
                        ...reply,
                        commenterName: reply.commenterName || existingReply?.commenterName || 'Anonymous'
                      };
                    }) || []
                  };
                }) || [];
                
                setComments(processedComments);
              }
            }
          }
        } catch (err) {
          console.error('Error refreshing comments:', err);
        }
      }
    } catch (err) {
      console.error('Error refreshing comments:', err);
    }
  };

  // Add a reply to a comment
  const [replyContent, setReplyContent] = useState('');
  const [replyName, setReplyName] = useState('');
  const [replyingTo, setReplyingTo] = useState(null);
  const [replyError, setReplyError] = useState('');

  const handleReplyClick = (commentId) => {
    setReplyingTo(commentId);
    // Pre-fill with the main commenter name if available
    const savedName = localStorage.getItem('commenterName');
    if (savedName) {
      setReplyName(savedName);
    } else if (commenterName) {
      setReplyName(commenterName);
    }
  };

  const handleCancelReply = () => {
    setReplyingTo(null);
    setReplyContent('');
    setReplyError('');
  };

  const handleSubmitReply = async (commentId) => {
    if (!replyContent.trim()) return;
    if (!replyName.trim()) {
      setReplyError('Please provide your name');
      return;
    }
    
    try {
      setReplyError('');
      
      // Store the reply name in localStorage for future use
      localStorage.setItem('commenterName', replyName);
      
      const response = await addPublicComment(token, replyContent, pageNumber, commentId, replyName);
      
      // Update comments list with the new reply
      setComments(prevComments => {
        const updatedComments = [...prevComments];
        const parentComment = updatedComments.find(c => c.id === commentId);
        
        if (parentComment) {
          if (!parentComment.replies) {
            parentComment.replies = [];
          }
          parentComment.replies.push({
            ...response,
            commenterName: replyName // Explicitly set the commenter name
          });
        }
        
        return updatedComments;
      });
      
      setReplyingTo(null);
      setReplyContent('');
      
      // Refresh comments to ensure we have the latest data
      setTimeout(() => refreshComments(), 500);
    } catch (err) {
      console.error('Error adding reply:', err);
      setReplyError('Failed to add reply. Please try again.');
    }
  };

  // Effect to refresh comments when page changes
  useEffect(() => {
    if (!loading && token) {
      refreshComments();
    }
  }, [pageNumber, token, loading]);

  // Filter comments for the current page
  const pageComments = comments.filter(comment => 
    comment.pageNumber === pageNumber && !comment.parentCommentId
  );

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleString();
  };

  if (loading) {
    return (
      <Container maxWidth="lg" sx={{ pt: 2 }}>
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="80vh">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ pt: 2 }}>
        <Box mt={5} textAlign="center">
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
          <Button 
            variant="contained" 
            color="primary"
            onClick={() => navigate('/')}
          >
            Go to Home
          </Button>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg" sx={{ pt: 2 }}>
      <Box mt={2} mb={4}>
        <Typography variant="h4" component="h1" gutterBottom align="center">
          {documentInfo ? documentInfo.fileName : 'Shared Document'}
          <Box component="span" sx={{ display: 'block', fontSize: '0.7em', color: 'text.secondary', mt: 1 }}>
            Shared Document View
          </Box>
        </Typography>
        
        <Paper elevation={3} sx={{ p: 3, mt: 3 }}>
          <Box className="pdf-container" display="flex" flexDirection="column" alignItems="center">
            {pdfError ? (
              <Alert severity="error" sx={{ mb: 2 }}>
                Failed to load PDF. The document may be corrupted or no longer available.
              </Alert>
            ) : (
              <>
                <Document
                  file={pdfUrl}
                  onLoadSuccess={onDocumentLoadSuccess}
                  onLoadError={onDocumentLoadError}
                  loading={<CircularProgress />}
                >
                  <Page 
                    pageNumber={pageNumber} 
                    className="pdf-page"
                    renderTextLayer={false}
                    renderAnnotationLayer={false}
                    width={Math.min(600, window.innerWidth - 50)}
                  />
                </Document>

                {numPages && (
                  <Box display="flex" alignItems="center" mt={2} mb={3}>
                    <Button 
                      onClick={handlePreviousPage} 
                      disabled={pageNumber <= 1}
                      variant="outlined"
                    >
                      Previous
                    </Button>
                    <Typography variant="body2" sx={{ mx: 2 }}>
                      Page {pageNumber} of {numPages}
                    </Typography>
                    <Button 
                      onClick={handleNextPage} 
                      disabled={pageNumber >= numPages}
                      variant="outlined"
                    >
                      Next
                    </Button>
                  </Box>
                )}
              </>
            )}
          </Box>
        </Paper>
        
        {/* Comments Section */}
        <Paper elevation={2} sx={{ p: 3, mt: 4 }}>
          <Typography variant="h5" gutterBottom>
            Comments {pageNumber && `(Page ${pageNumber})`}
          </Typography>
          
          {commentError && <Alert severity="error" sx={{ mb: 2 }}>{commentError}</Alert>}
          
          <Box mb={3}>
            <TextField
              fullWidth
              label="Your Name"
              placeholder="Enter your name"
              value={commenterName}
              onChange={(e) => setCommenterName(e.target.value)}
              variant="outlined"
              margin="normal"
              required
              error={commentError && !commenterName.trim()}
              helperText={commentError && !commenterName.trim() ? "Name is required" : ""}
            />
            <TextField
              fullWidth
              multiline
              rows={3}
              placeholder="Add a comment about this document..."
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              variant="outlined"
              margin="normal"
            />
            <Box display="flex" justifyContent="flex-end" mt={1}>
              <Button 
                variant="contained" 
                color="primary" 
                onClick={handleAddComment}
                disabled={!newComment.trim() || !commenterName.trim()}
              >
                Add Comment
              </Button>
            </Box>
          </Box>
          
          <Divider sx={{ mb: 3 }} />
          
          {pageComments.length === 0 ? (
            <Typography variant="body2" color="textSecondary" align="center">
              No comments on this page yet. Be the first to comment!
            </Typography>
          ) : (
            pageComments.map(comment => (
              <Card key={comment.id} sx={{ mb: 2 }}>
                <CardContent>
                  <Box display="flex" justifyContent="space-between" alignItems="center">
                    <Typography variant="subtitle2" color="primary">
                      {comment.commenterName || 'Anonymous'}
                    </Typography>
                  </Box>
                  
                  <Typography variant="body1" sx={{ mt: 1 }}>
                    {comment.content}
                  </Typography>
                  
                  <Typography variant="caption" color="textSecondary" display="block" sx={{ mt: 1 }}>
                    {formatDate(comment.createdAt)}
                  </Typography>
                </CardContent>
                
                <CardActions>
                  <Button 
                    size="small" 
                    variant="text" 
                    color="primary"
                    onClick={() => handleReplyClick(comment.id)}
                  >
                    Reply
                  </Button>
                </CardActions>
                
                {/* Reply form */}
                {replyingTo === comment.id && (
                  <Box sx={{ p: 2, bgcolor: '#f5f5f5' }}>
                    {replyError && <Alert severity="error" sx={{ mb: 2 }}>{replyError}</Alert>}
                    <TextField
                      fullWidth
                      label="Your Name"
                      placeholder="Enter your name"
                      value={replyName}
                      onChange={(e) => setReplyName(e.target.value)}
                      variant="outlined"
                      margin="normal"
                      size="small"
                      required
                      error={replyError && !replyName.trim()}
                    />
                    <TextField
                      fullWidth
                      multiline
                      rows={2}
                      placeholder="Write your reply..."
                      value={replyContent}
                      onChange={(e) => setReplyContent(e.target.value)}
                      variant="outlined"
                      margin="normal"
                      size="small"
                    />
                    <Box display="flex" justifyContent="flex-end" mt={1}>
                      <Button 
                        size="small" 
                        onClick={handleCancelReply} 
                        sx={{ mr: 1 }}
                      >
                        Cancel
                      </Button>
                      <Button 
                        size="small" 
                        variant="contained" 
                        color="primary"
                        onClick={() => handleSubmitReply(comment.id)}
                        disabled={!replyContent.trim() || !replyName.trim()}
                      >
                        Submit Reply
                      </Button>
                    </Box>
                  </Box>
                )}
                
                {/* Render replies */}
                {comment.replies && comment.replies.length > 0 && (
                  <Box sx={{ pl: 2, pr: 2, pb: 1 }}>
                    {comment.replies.map(reply => (
                      <Card key={reply.id} sx={{ mb: 1, backgroundColor: '#f5f5f5' }}>
                        <CardContent>
                          <Typography variant="subtitle2" color="primary">
                            {reply.commenterName || 'Anonymous'}
                          </Typography>
                          <Typography variant="body2">
                            {reply.content}
                          </Typography>
                          <Typography variant="caption" color="textSecondary">
                            {formatDate(reply.createdAt)}
                          </Typography>
                        </CardContent>
                      </Card>
                    ))}
                  </Box>
                )}
              </Card>
            ))
          )}
        </Paper>
      </Box>
    </Container>
  );
};

export default SharedPdfViewer;
