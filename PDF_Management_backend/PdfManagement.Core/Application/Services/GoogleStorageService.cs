using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PdfManagement.Core.Application.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace PdfManagement.Core.Application.Services
{
    public class GoogleStorageService : IGoogleStorageService
    {
        private readonly DriveService _driveService;
        private readonly string _googleFolderId;
        private readonly ILogger<GoogleStorageService> _logger;

        public GoogleStorageService(IConfiguration config, ILogger<GoogleStorageService> logger)
        {
            _logger = logger;
            
            // Get folder ID from environment variable or config
            _googleFolderId = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_FOLDER_ID") ?? 
                             config["GoogleDrive:FolderId"]; // The Drive folder to upload to
            
            _logger.LogInformation($"Using Google Drive folder ID: {_googleFolderId}");

            try
            {
                // Check for environment variables first
                string clientEmail = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_EMAIL");
                string privateKey = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY");
                
                GoogleCredential credential;
                
                if (!string.IsNullOrEmpty(clientEmail) && !string.IsNullOrEmpty(privateKey))
                {
                    _logger.LogInformation("Using Google credentials from environment variables");
                    
                    // Create credentials from environment variables
                    var serviceAccountCredentialInitializer = new ServiceAccountCredential.Initializer(clientEmail)
                    {
                        ProjectId = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID"),
                        KeyId = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY_ID")
                    }.FromPrivateKey(privateKey);
                    
                    var serviceAccountCredential = new ServiceAccountCredential(serviceAccountCredentialInitializer);
                    credential = GoogleCredential.FromServiceAccountCredential(serviceAccountCredential)
                        .CreateScoped(DriveService.Scope.DriveFile);
                }
                else
                {
                    // Fall back to file-based credentials if environment variables aren't available
                    var serviceAccountPath = config["GoogleDrive:ServiceAccountPath"];
                    _logger.LogInformation($"Initializing Google Storage Service with service account file: {serviceAccountPath}");
                    
                    if (string.IsNullOrEmpty(serviceAccountPath))
                    {
                        throw new ArgumentException("Google Drive service account path is not configured and environment variables are not set");
                    }

                    if (!File.Exists(serviceAccountPath))
                    {
                        // Try to find the pdf_service.txt file as fallback
                        var pdfServicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pdf_service.txt");
                        if (File.Exists(pdfServicePath))
                        {
                            serviceAccountPath = pdfServicePath;
                            _logger.LogWarning($"Using fallback credentials file: {pdfServicePath}");
                        }
                        else
                        {
                            throw new FileNotFoundException($"Google Drive service account file not found at {serviceAccountPath}");
                        }
                    }

                    using (var stream = new FileStream(serviceAccountPath, FileMode.Open, FileAccess.Read))
                    {
                        credential = GoogleCredential.FromStream(stream)
                            .CreateScoped(DriveService.Scope.DriveFile);
                    }
                }

                _driveService = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "PdfManagement"
                });
                
                _logger.LogInformation("Google Drive service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Google Drive service");
                throw;
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string userId)
        {
            try
            {
                if (file == null)
                {
                    throw new ArgumentNullException(nameof(file), "File cannot be null");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    userId = "anonymous";
                    _logger.LogWarning("User ID is null or empty, using 'anonymous' as default");
                }

                _logger.LogInformation($"Saving file {file.FileName} for user {userId}");
                
                // Create a folder for the user if it doesn't exist
                string userFolderId = await GetOrCreateUserFolderAsync(userId);
                
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = $"{Guid.NewGuid()}_{file.FileName}", // Generate unique filename
                    Parents = new[] { userFolderId }
                };

                using var stream = file.OpenReadStream();
                var request = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
                request.Fields = "id";

                var result = await request.UploadAsync();

                if (result.Status == UploadStatus.Completed)
                {
                    _logger.LogInformation($"File uploaded successfully with ID: {request.ResponseBody.Id}");
                    // Return a path that includes the user ID for consistency with local storage
                    return $"{userId}/{request.ResponseBody.Id}";
                }

                _logger.LogError($"Failed to upload file: {result.Exception?.Message}");
                throw new Exception($"Failed to upload file: {result.Exception?.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving file {file?.FileName}");
                throw;
            }
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogError("GetFileAsync called with null or empty file path");
                    throw new ArgumentNullException(nameof(filePath), "File path cannot be null or empty");
                }
                
                _logger.LogInformation($"Getting file: {filePath}");
                
                // Extract the file ID from the path (userId/fileId)
                string fileId = ExtractFileIdFromPath(filePath);
                
                var request = _driveService.Files.Get(fileId);
                using var ms = new MemoryStream();
                await request.DownloadAsync(ms);
                
                _logger.LogInformation($"File downloaded successfully, size: {ms.Length} bytes");
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file {filePath}");
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogError("DeleteFileAsync called with null or empty file path");
                    return false;
                }
                
                _logger.LogInformation($"Deleting file: {filePath}");
                
                // Extract the file ID from the path (userId/fileId)
                string fileId = ExtractFileIdFromPath(filePath);
                
                var request = _driveService.Files.Delete(fileId);
                await request.ExecuteAsync();
                
                _logger.LogInformation($"File deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {filePath}");
                return false;
            }
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogError("FileExistsAsync called with null or empty file path");
                    return false;
                }
                
                _logger.LogInformation($"Checking if file exists: {filePath}");
                
                // Extract the file ID from the path (userId/fileId)
                string fileId = ExtractFileIdFromPath(filePath);
                
                var request = _driveService.Files.Get(fileId);
                request.Fields = "id";
                var file = await request.ExecuteAsync();
                
                bool exists = file != null;
                _logger.LogInformation($"File exists: {exists}");
                return exists;
            }
            catch (Exception)
            {
                _logger.LogInformation($"File does not exist: {filePath}");
                return false;
            }
        }

        #region Helper Methods
        
        private async Task<string> GetOrCreateUserFolderAsync(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(_googleFolderId))
                {
                    _logger.LogWarning("Google Drive folder ID is not configured, using root folder");
                    // If no folder ID is configured, create folders at the root level
                    return await CreateUserFolderAtRootAsync(userId);
                }

                // First check if the user folder already exists
                var searchRequest = _driveService.Files.List();
                searchRequest.Q = $"name = '{userId}' and mimeType = 'application/vnd.google-apps.folder' and '{_googleFolderId}' in parents and trashed = false";
                searchRequest.Fields = "files(id, name)";
                
                var searchResult = await searchRequest.ExecuteAsync();
                
                if (searchResult.Files != null && searchResult.Files.Count > 0)
                {
                    _logger.LogInformation($"Found existing folder for user {userId}: {searchResult.Files[0].Id}");
                    return searchResult.Files[0].Id;
                }
                
                // Create a new folder for the user
                var folderMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = userId,
                    MimeType = "application/vnd.google-apps.folder",
                    Parents = new[] { _googleFolderId }
                };
                
                var createRequest = _driveService.Files.Create(folderMetadata);
                createRequest.Fields = "id";
                var folder = await createRequest.ExecuteAsync();
                
                _logger.LogInformation($"Created new folder for user {userId}: {folder.Id}");
                return folder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating/getting folder for user {userId}");
                throw;
            }
        }

        private async Task<string> CreateUserFolderAtRootAsync(string userId)
        {
            try
            {
                // Check if the user folder already exists at root
                var searchRequest = _driveService.Files.List();
                searchRequest.Q = $"name = '{userId}' and mimeType = 'application/vnd.google-apps.folder' and 'root' in parents and trashed = false";
                searchRequest.Fields = "files(id, name)";
                
                var searchResult = await searchRequest.ExecuteAsync();
                
                if (searchResult.Files != null && searchResult.Files.Count > 0)
                {
                    _logger.LogInformation($"Found existing folder for user {userId} at root: {searchResult.Files[0].Id}");
                    return searchResult.Files[0].Id;
                }
                
                // Create a new folder for the user at root
                var folderMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = userId,
                    MimeType = "application/vnd.google-apps.folder"
                    // No Parents means it will be created at root
                };
                
                var createRequest = _driveService.Files.Create(folderMetadata);
                createRequest.Fields = "id";
                var folder = await createRequest.ExecuteAsync();
                
                _logger.LogInformation($"Created new folder for user {userId} at root: {folder.Id}");
                return folder.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating folder for user {userId} at root");
                throw;
            }
        }
        
        private string ExtractFileIdFromPath(string filePath)
        {
            // The filePath is expected to be in the format "userId/fileId"
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }
            
            var parts = filePath.Split('/');
            if (parts.Length < 2)
            {
                // If there's no userId prefix, assume the entire string is the fileId
                return filePath;
            }
            
            // Return the last part as the fileId
            return parts[parts.Length - 1];
        }
        
        #endregion
    }
}
