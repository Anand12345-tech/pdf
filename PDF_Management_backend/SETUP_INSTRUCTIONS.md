# PDF Management Backend Setup Instructions

## Google Drive Integration

The application has been configured to use Google Drive for PDF storage using a service account approach. The service account credentials have been saved to:

```
/mnt/d/pdf_management/repo/PDF_Management_backend/credentials/service-account.json
```

### Configuration

The following environment variables have been set in the `.env` file:

```
FILE_STORAGE_PROVIDER=GoogleDrive
GOOGLE_DRIVE_CREDENTIALS_PATH=/mnt/d/pdf_management/repo/PDF_Management_backend/credentials/service-account.json
GOOGLE_DRIVE_FOLDER_ID=1i-mIv6TtvteoyCZSN9DSJt0B3QEFPfeB
GOOGLE_CLIENT_EMAIL=pdf-drive-uploader@pdfmanagement-462009.iam.gserviceaccount.com
GOOGLE_CLIENT_ID=112237127959889365362
```

### Testing the Integration

1. Start your application with:
   ```
   dotnet run --project PdfManagement.API
   ```

2. Test the connection by accessing:
   ```
   http://localhost:5001/api/drive/test
   ```

3. To upload a file, send a POST request to:
   ```
   http://localhost:5001/api/drive/upload
   ```
   with a file in the form data.

4. To download a file, send a GET request to:
   ```
   http://localhost:5001/api/drive/download/{fileId}
   ```
   where `{fileId}` is the ID returned from the upload endpoint.

## Troubleshooting

### Socket Exception

If you encounter a `System.Net.Sockets.SocketException` with the message 'An attempt was made to access a socket in a way forbidden by its access permissions', this is likely because:

1. The application is trying to use port 5000 which may be in use by another service
2. Windows often reserves port 5000 for the "Windows Process Activation Service"

The application has been configured to use port 5001 instead in the `appsettings.json` file:

```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://localhost:5001"
    }
  }
}
```

### Google Drive Access Issues

If you encounter issues accessing Google Drive:

1. Make sure the service account has been granted access to the Google Drive folder
2. Verify that the Google Drive API is enabled for your project
3. Check the application logs for detailed error messages

To share a folder with the service account:
1. Go to Google Drive
2. Right-click on the folder
3. Select "Share"
4. Add the service account email: `pdf-drive-uploader@pdfmanagement-462009.iam.gserviceaccount.com`
5. Grant "Editor" access

### Debugging

The application has been configured with extensive logging to help diagnose issues. Check the console output for detailed error messages.
