const axios = require('axios');
const fs = require('fs');
const path = require('path');

// Document ID to test
const documentId = 1; // Change this to a valid document ID in your system

async function testDownload() {
  try {
    console.log(`Testing download for document ID: ${documentId}`);
    console.log('Sending request to: http://localhost:5000/api/PdfDocuments/download/' + documentId);
    
    const response = await axios.get(`http://localhost:5000/api/PdfDocuments/download/${documentId}`, {
      responseType: 'arraybuffer'
    });
    
    console.log('Download successful!');
    console.log('Status:', response.status);
    console.log('Headers:', JSON.stringify(response.headers));
    console.log('Content-Type:', response.headers['content-type']);
    console.log('Content-Length:', response.headers['content-length']);
    console.log('Content-Disposition:', response.headers['content-disposition']);
    
    // Save the file to disk for verification
    const fileName = 'test-download.pdf';
    const filePath = path.join(__dirname, fileName);
    fs.writeFileSync(filePath, Buffer.from(response.data));
    
    console.log(`File saved to: ${filePath}`);
  } catch (error) {
    console.error('Download failed!');
    if (error.response) {
      console.error('Status:', error.response.status);
      console.error('Headers:', JSON.stringify(error.response.headers));
      console.error('Data:', error.response.data.toString());
    } else if (error.request) {
      console.error('No response received. Backend might not be running or accessible.');
    } else {
      console.error('Error:', error.message);
    }
  }
}

testDownload();
