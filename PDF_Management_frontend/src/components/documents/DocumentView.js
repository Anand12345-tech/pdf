import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { 
  Box, 
  Typography, 
  Paper, 
  CircularProgress, 
  Button,
  Divider,
  Alert,
  IconButton,
  Tooltip
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { Document, Page, pdfjs } from 'react-pdf';
import CommentSection from './CommentSection';
import { getDocument, getDocumentComments, viewDocument, downloadDocument } from '../../services/api';

// Set up the worker for react-pdf
pdfjs.GlobalWorkerOptions.workerSrc = `//cdnjs.cloudflare.com/ajax/libs/pdf.js/${pdfjs.version}/pdf.worker.min.js`;

const DocumentView = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const [document, setDocument] = useState(null);
  const [comments, setComments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [numPages, setNumPages] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [pdfError, setPdfError] = useState(false);

  useEffect(() => {
    const fetchDocumentData = async () => {
      try {
        setLoading(true);
        const documentData = await getDocument(id);
        setDocument(documentData);
        
        const pdfUrl = await viewDocument(id);
        setDocument(prev => ({ ...prev, viewUrl: pdfUrl }));

        const commentsData = await getDocumentComments(id);
        
        // Process comments to ensure commenterName is properly used
        const processedComments = commentsData.map(comment => {
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
          
          return {
            ...comment,
            commenterName: comment.commenterName || storedNames[comment.id] || comment.commenter?.userName || 'Anonymous',
            replies: comment.replies?.map(reply => ({
              ...reply,
              commenterName: reply.commenterName || storedNames[reply.id] || reply.commenter?.userName || 'Anonymous'
            })) || []
          };
        });
        
        setComments(processedComments);
        
        setError('');
      } catch (err) {
        console.error('Error fetching document data:', err);
        setError('Failed to load document. It may have been deleted or you don\'t have permission to view it.');
      } finally {
        setLoading(false);
      }
    };

    fetchDocumentData();
  }, [id]);

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

  const handleCommentAdded = (newComment) => {
    // Store the commenter name if available
    if (newComment.commenterName) {
      try {
        // Get existing stored names
        const storedNames = localStorage.getItem('commentNames') || '{}';
        const commentNames = JSON.parse(storedNames);
        
        // Add this comment's name
        commentNames[newComment.id] = newComment.commenterName;
        
        // Save back to localStorage
        localStorage.setItem('commentNames', JSON.stringify(commentNames));
      } catch (e) {
        console.error('Error storing comment name:', e);
      }
    }
    
    if (newComment.parentCommentId) {
      // This is a reply to an existing comment
      setComments(prevComments =>
        prevComments.map(comment => {
          if (comment.id === newComment.parentCommentId) {
            // Add the reply to the parent comment's replies array
            return {
              ...comment,
              replies: [...(comment.replies || []), newComment]
            };
          } else {
            return comment;
          }
        })
      );
    } else {
      // This is a new top-level comment
      setComments(prevComments => [...prevComments, newComment]);
    }
  };

 const handleCommentUpdated = (updatedComment) => {
  setComments(prevComments =>
    prevComments.map(comment => {
      // If this is the comment being updated
      if (comment.id === updatedComment.id) {
        return updatedComment;
      }

      // Check if the updated comment is in the replies
      if (comment.replies && comment.replies.length > 0) {
        return {
          ...comment,
          replies: comment.replies.map(reply =>
            reply.id === updatedComment.id ? updatedComment : reply
          )
        };
      }

      return comment;
    })
  );
};


const handleCommentDeleted = (commentId) => {
  setComments(prevComments =>
    prevComments.map(comment => {
      // If this is the comment being deleted
      if (comment.id === commentId) {
        return null; // Will be filtered out below
      }

      // Check if the deleted comment is in the replies
      if (comment.replies && comment.replies.length > 0) {
        return {
          ...comment,
          replies: comment.replies.filter(reply => reply.id !== commentId)
        };
      }

      return comment;
    }).filter(Boolean) // Remove any null values (deleted top-level comments)
  );
};

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="50vh">
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return (
      <Box mt={3}>
        <Alert severity="error">{error}</Alert>
        <Button 
          startIcon={<ArrowBackIcon />} 
          onClick={() => navigate('/dashboard')}
          sx={{ mt: 2 }}
        >
          Back to Dashboard
        </Button>
      </Box>
    );
  }

  return (
    <Box>
      <Box display="flex" alignItems="center" mb={3}>
        <IconButton onClick={() => navigate('/dashboard')} sx={{ mr: 1 }}>
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h5" component="h1">
          {document?.fileName}
        </Typography>
      </Box>

      <Paper elevation={2} sx={{ p: 3, mb: 4 }}>
        <Box className="pdf-container">
          {pdfError ? (
            <Alert severity="error" sx={{ mb: 2 }}>
              Failed to load PDF. Please try downloading the file instead.
            </Alert>
          ) : (
            <>
              <Document
                file={document?.viewUrl}
                onLoadSuccess={onDocumentLoadSuccess}
                onLoadError={onDocumentLoadError}
                loading={<CircularProgress />}
              >
                <Page 
                  pageNumber={pageNumber} 
                  className="pdf-page"
                  renderTextLayer={false}
                  renderAnnotationLayer={false}
                />
              </Document>

              {numPages && (
                <Box display="flex" alignItems="center" mt={2} mb={3}>
                  <Button 
                    onClick={handlePreviousPage} 
                    disabled={pageNumber <= 1}
                  >
                    Previous
                  </Button>
                  <Typography variant="body2" sx={{ mx: 2 }}>
                    Page {pageNumber} of {numPages}
                  </Typography>
                  <Button 
                    onClick={handleNextPage} 
                    disabled={pageNumber >= numPages}
                  >
                    Next
                  </Button>
                </Box>
              )}
            </>
          )}

          <Box mt={2} display="flex" justifyContent="center">
            <Tooltip title="Download the original PDF file">
              <Button 
                variant="outlined" 
                color="primary"
                onClick={() => {
                  try {
                    setLoading(true);
                    
                    // Get API base URL from environment variables
                    const apiBaseUrl = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';
                    // Remove trailing /api if present to construct the correct URL
                    const baseUrl = apiBaseUrl.endsWith('/api') 
                      ? apiBaseUrl.slice(0, -4) 
                      : apiBaseUrl.replace('/api', '');
                    
                    const downloadUrl = `${baseUrl}/api/PdfDocuments/download/${id}`;
                    
                    // Open in a new tab which will trigger the download
                    window.open(downloadUrl, '_blank');
                    
                    setTimeout(() => {
                      setLoading(false);
                    }, 1000);
                  } catch (error) {
                    console.error('Download failed:', error);
                    setError('Failed to download the document. Please try again later.');
                    setLoading(false);
                  }
                }}
              >
                Download PDF
              </Button>
            </Tooltip>
          </Box>
        </Box>
      </Paper>

      <Divider sx={{ my: 3 }} />

      <CommentSection 
        documentId={id}
        comments={comments}
        currentPage={pageNumber}
        onCommentAdded={handleCommentAdded}
        onCommentUpdated={handleCommentUpdated}
        onCommentDeleted={handleCommentDeleted}
      />
    </Box>
  );
};

export default DocumentView;
