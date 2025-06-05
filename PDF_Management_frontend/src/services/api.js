import axios from 'axios';

// Configure axios defaults
const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';
axios.defaults.baseURL = API_URL;

// Remove any trailing /api from the baseURL if it's already in the API_URL
if (axios.defaults.baseURL.endsWith('/api')) {
  axios.defaults.baseURL = axios.defaults.baseURL.replace(/\/api$/, '');
}

// Add request interceptor to include auth token
axios.interceptors.request.use(
  config => {
    const token = localStorage.getItem(process.env.REACT_APP_AUTH_TOKEN_KEY || 'pdf_management_auth_token');
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`;
    }
    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

// Add response interceptor for error handling
axios.interceptors.response.use(
  response => response,
  error => {
    // Handle network errors
    if (!error.response) {
      console.error('Network error:', error);
      return Promise.reject(new Error('Network error. Please check your connection.'));
    }
    
    // Handle authentication errors
    if (error.response.status === 401) {
      // Clear token and redirect to login if unauthorized
      localStorage.removeItem(process.env.REACT_APP_AUTH_TOKEN_KEY || 'pdf_management_auth_token');
      if (window.location.pathname !== '/login') {
        window.location.href = '/login';
      }
    }
    
    return Promise.reject(error);
  }
);

// Document APIs
export const getDocuments = async () => {
  try {
    const response = await axios.get('/api/PdfDocuments');
    return response.data;
  } catch (error) {
    console.error('Error fetching documents:', error);
    throw error;
  }
};

export const getDocument = async (id) => {
  try {
    const response = await axios.get(`/api/PdfDocuments/${id}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching document ${id}:`, error);
    throw error;
  }
};

export const uploadDocument = async (file) => {
  try {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await axios.post('/api/PdfDocuments', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    return response.data;
  } catch (error) {
    console.error('Error uploading document:', error);
    throw error;
  }
};

export const viewDocument = async (id) => {
  try {
    const response = await axios.get(`/api/PdfDocuments/view/${id}`, {
      responseType: 'blob'
    });
    return URL.createObjectURL(response.data);
  } catch (error) {
    console.error(`Error viewing document ${id}:`, error);
    throw error;
  }
};

export const downloadDocument = async (id) => {
  try {
    console.log(`Attempting to download document ${id}`);
    const response = await axios.get(`/api/PdfDocuments/download/${id}`, {
      responseType: 'blob'
    });
    console.log('Download response received:', response.status, response.headers);
    return URL.createObjectURL(response.data);
  } catch (error) {
    console.error(`Error downloading document ${id}:`, error);
    if (error.response) {
      console.error('Response status:', error.response.status);
      console.error('Response headers:', error.response.headers);
    }
    throw error;
  }
};

export const deleteDocument = async (id) => {
  try {
    await axios.delete(`/api/PdfDocuments/${id}`);
    return true;
  } catch (error) {
    console.error(`Error deleting document ${id}:`, error);
    throw error;
  }
};

export const shareDocument = async (id, expiresAt) => {
  try {
    const response = await axios.post(`/api/PdfDocuments/${id}/share`, { expiresAt });
    return response.data;
  } catch (error) {
    console.error(`Error sharing document ${id}:`, error);
    throw error;
  }
};

// Comment APIs
export const getDocumentComments = async (documentId) => {
  try {
    const response = await axios.get(`/api/Comments/document/${documentId}`);
    
    // Process comments to ensure commenterName is properly used
    const comments = response.data || [];
    
    // Get any stored comment names from localStorage
    const storedCommentNames = {};
    try {
      const storedNames = localStorage.getItem('commentNames');
      if (storedNames) {
        Object.assign(storedCommentNames, JSON.parse(storedNames));
      }
    } catch (e) {
      console.error('Error parsing stored comment names:', e);
    }
    
    // Process comments to ensure names are properly displayed
    return comments.map(comment => {
      // Use stored name if available, otherwise use API value or default to Anonymous
      const storedName = storedCommentNames[comment.id];
      return {
        ...comment,
        commenterName: comment.commenterName || storedName || comment.commenter?.userName || 'Anonymous',
        replies: comment.replies?.map(reply => {
          const storedReplyName = storedCommentNames[reply.id];
          return {
            ...reply,
            commenterName: reply.commenterName || storedReplyName || reply.commenter?.userName || 'Anonymous'
          };
        }) || []
      };
    });
  } catch (error) {
    console.error(`Error fetching comments for document ${documentId}:`, error);
    throw error;
  }
};

export const getCommentReplies = async (commentId) => {
  try {
    const response = await axios.get(`/api/Comments/replies/${commentId}`);
    return response.data;
  } catch (error) {
    console.error(`Error fetching replies for comment ${commentId}:`, error);
    throw error;
  }
};

export const addComment = async (documentId, content, pageNumber, parentCommentId = null) => {
  try {
    const response = await axios.post(`/api/Comments/document/${documentId}`, {
      content,
      pageNumber,
      parentCommentId
    });
    return response.data;
  } catch (error) {
    console.error('Error adding comment:', error);
    throw error;
  }
};

export const updateComment = async (commentId, content) => {
  try {
    const response = await axios.put(`/api/Comments/${commentId}`, { content });
    return response.data;
  } catch (error) {
    console.error(`Error updating comment ${commentId}:`, error);
    throw error;
  }
};

export const deleteComment = async (commentId) => {
  try {
    await axios.delete(`/api/Comments/${commentId}`);
    return true;
  } catch (error) {
    console.error(`Error deleting comment ${commentId}:`, error);
    throw error;
  }
};

export const shareDocumentJwt = async (id, expiresAt) => {
  try {
    const response = await axios.post(`/api/PdfDocuments/${id}/share-jwt`, { expiresAt });
    return response.data;
  } catch (error) {
    console.error(`Error sharing document ${id} with JWT:`, error);
    throw error;
  }
};

export const viewSharedDocumentJwt = async (token) => {
  try {
    console.log('Fetching document with JWT token:', token);
    const response = await axios.get(`/api/Public/view-jwt/${token}`, {
      responseType: 'blob'
    });
    console.log('JWT document response received:', response.status);
    return URL.createObjectURL(response.data);
  } catch (error) {
    console.error(`Error viewing shared document with JWT:`, error);
    if (error.response) {
      console.error('Response status:', error.response.status);
      console.error('Response headers:', error.response.headers);
    }
    throw error;
  }
};

export const addPublicComment = async (token, content, pageNumber, parentCommentId = null, commenterName = null) => {
  try {
    // Check if token is a UUID or JWT
    const isUuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(token);
    
    const endpoint = isUuid 
      ? `/api/Public/comment/${token}`
      : `/api/Public/comment-jwt/${token}`;
    
    console.log('Adding public comment to endpoint:', endpoint);
    console.log('Comment data:', { content, pageNumber, parentCommentId, commenterName });
      
    const response = await axios.post(endpoint, {
      content,
      pageNumber,
      parentCommentId,
      commenterName
    });
    
    console.log('Comment response:', response.status, response.data);
    
    // Store the commenter name for this comment ID in localStorage
    if (response.data && response.data.comment && response.data.comment.id && commenterName) {
      try {
        // Get existing stored names
        const storedNames = localStorage.getItem('commentNames') || '{}';
        const commentNames = JSON.parse(storedNames);
        
        // Add this comment's name
        commentNames[response.data.comment.id] = commenterName;
        
        // Save back to localStorage
        localStorage.setItem('commentNames', JSON.stringify(commentNames));
      } catch (e) {
        console.error('Error storing comment name:', e);
      }
    }
    
    return response.data;
  } catch (error) {
    console.error('Error adding public comment:', error);
    if (error.response) {
      console.error('Response status:', error.response.status);
      console.error('Response data:', error.response.data);
    }
    throw error;
  }
};

export const getPublicComments = async (token) => {
  try {
    const response = await axios.get(`/api/Public/view/${token}`);
    
    // Process comments to ensure commenterName is properly used
    const comments = response.data.comments || [];
    
    // Get any stored comment names from localStorage
    const storedCommentNames = {};
    try {
      const storedNames = localStorage.getItem('commentNames');
      if (storedNames) {
        Object.assign(storedCommentNames, JSON.parse(storedNames));
      }
    } catch (e) {
      console.error('Error parsing stored comment names:', e);
    }
    
    return comments.map(comment => {
      // Use stored name if available, otherwise use API value or default to Anonymous
      const storedName = storedCommentNames[comment.id];
      return {
        ...comment,
        commenterName: comment.commenterName || storedName || 'Anonymous',
        replies: comment.replies?.map(reply => {
          const storedReplyName = storedCommentNames[reply.id];
          return {
            ...reply,
            commenterName: reply.commenterName || storedReplyName || 'Anonymous'
          };
        }) || []
      };
    });
  } catch (error) {
    console.error(`Error fetching public comments for token ${token}:`, error);
    throw error;
  }
};
