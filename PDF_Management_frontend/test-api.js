const axios = require('axios');

async function testApi() {
  try {
    console.log('Testing API connectivity...');
    console.log('Attempting to connect to: http://localhost:5000/api/PdfDocuments');
    
    const response = await axios.get('http://localhost:5000/api/PdfDocuments', {
      timeout: 5000
    });
    
    console.log('Connection successful!');
    console.log('Status:', response.status);
    console.log('Data:', JSON.stringify(response.data).substring(0, 100) + '...');
  } catch (error) {
    console.error('Connection failed!');
    if (error.response) {
      console.error('Status:', error.response.status);
      console.error('Headers:', JSON.stringify(error.response.headers));
    } else if (error.request) {
      console.error('No response received. Backend might not be running or accessible.');
    } else {
      console.error('Error:', error.message);
    }
  }
}

testApi();
