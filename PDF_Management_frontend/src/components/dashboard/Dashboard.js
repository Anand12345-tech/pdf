import React, { useState, useEffect } from 'react';
import { 
  Typography, 
  Grid, 
  Button, 
  Box, 
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { getDocuments, uploadDocument } from '../../services/api';
import DocumentCard from './DocumentCard';

const Dashboard = () => {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [uploadOpen, setUploadOpen] = useState(false);
  const [selectedFile, setSelectedFile] = useState(null);
  const [uploading, setUploading] = useState(false);

  useEffect(() => {
    fetchDocuments();
  }, []);

  const fetchDocuments = async () => {
    try {
      setLoading(true);
      const data = await getDocuments();
      setDocuments(data);
      setError('');
    } catch (err) {
      setError('Failed to load documents. Please try again later.');
      console.error('Error fetching documents:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (file && file.type === 'application/pdf') {
      setSelectedFile(file);
    } else {
      setSelectedFile(null);
      alert('Please select a valid PDF file');
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;

    try {
      setUploading(true);
      await uploadDocument(selectedFile);
      setUploadOpen(false);
      setSelectedFile(null);
      fetchDocuments(); // Refresh the document list
    } catch (err) {
      console.error('Error uploading document:', err);
      alert('Failed to upload document. Please try again.');
    } finally {
      setUploading(false);
    }
  };

  const handleDocumentDeleted = () => {
    fetchDocuments(); // Refresh the document list after deletion
  };

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1">
          My Documents
        </Typography>
        <Button 
          variant="contained" 
          color="primary" 
          startIcon={<AddIcon />}
          onClick={() => setUploadOpen(true)}
        >
          Upload PDF
        </Button>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}

      {loading ? (
        <Box display="flex" justifyContent="center" my={5}>
          <CircularProgress />
        </Box>
      ) : documents.length === 0 ? (
        <Alert severity="info">
          You don't have any documents yet. Upload your first PDF document!
        </Alert>
      ) : (
        <Grid container spacing={3}>
          {documents.map((document) => (
            <Grid item xs={12} sm={6} md={4} key={document.id}>
              <DocumentCard document={document} onDeleted={handleDocumentDeleted} />
            </Grid>
          ))}
        </Grid>
      )}

      {/* Upload Dialog */}
      <Dialog open={uploadOpen} onClose={() => setUploadOpen(false)}>
        <DialogTitle>Upload PDF Document</DialogTitle>
        <DialogContent>
          <Box mt={2}>
            <input
              accept="application/pdf"
              style={{ display: 'none' }}
              id="upload-pdf-file"
              type="file"
              onChange={handleFileChange}
            />
            <label htmlFor="upload-pdf-file">
              <Button variant="outlined" component="span" fullWidth>
                Select PDF File
              </Button>
            </label>
            {selectedFile && (
              <Typography variant="body2" mt={1}>
                Selected: {selectedFile.name} ({Math.round(selectedFile.size / 1024)} KB)
              </Typography>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUploadOpen(false)} disabled={uploading}>
            Cancel
          </Button>
          <Button 
            onClick={handleUpload} 
            color="primary" 
            disabled={!selectedFile || uploading}
          >
            {uploading ? 'Uploading...' : 'Upload'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Dashboard;
