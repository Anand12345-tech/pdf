using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Data.Models;
using PdfManagement.Data.Repositories.Interfaces;
using PdfManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PdfManagement.Services.Implementations
{
    /// <summary>
    /// Implementation of the PDF document service using Google Cloud Storage
    /// </summary>
    public class PdfDocumentService : IPdfDocumentService
    {
        private readonly IPdfDocumentRepository _pdfDocumentRepository;
        private readonly IGoogleStorageService _googleStorageService;
        private readonly ILogger<PdfDocumentService> _logger;

        public PdfDocumentService(
            IPdfDocumentRepository pdfDocumentRepository,
            IGoogleStorageService googleStorageService,
            ILogger<PdfDocumentService> logger)
        {
            _pdfDocumentRepository = pdfDocumentRepository;
            _googleStorageService = googleStorageService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<PdfDocument> UploadPdfAsync(IFormFile file, string userId)
        {
            _logger.LogInformation($"Uploading PDF file {file.FileName} for user {userId}");
            
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded or file is empty");
                throw new ArgumentException("No file uploaded.");
            }

            if (file.ContentType != "application/pdf")
            {
                _logger.LogWarning($"Invalid file type: {file.ContentType}");
                throw new ArgumentException("Only PDF files are allowed.");
            }

            try
            {
                // Save the file to Google Cloud Storage
                var filePath = await _googleStorageService.SaveFileAsync(file, userId);
                _logger.LogInformation($"File saved to Google Cloud Storage with path: {filePath}");
                
                // Create document record
                var document = new PdfDocument
                {
                    FileName = Path.GetFileName(file.FileName),
                    FilePath = filePath,
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    UploaderId = userId,
                    UploadedAt = DateTime.UtcNow,
                    StorageProvider = "GoogleCloud" // Add storage provider information
                };
                
                // Save to database
                var result = await _pdfDocumentRepository.AddAsync(document);
                _logger.LogInformation($"PDF document record created with ID: {result.Id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading PDF: {ex.Message}");
                throw new Exception($"Failed to upload PDF: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<PdfDocument> GetPdfByIdAsync(int id)
        {
            _logger.LogInformation($"Getting PDF document with ID: {id}");
            return await _pdfDocumentRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PdfDocument>> GetUserPdfsAsync(string userId)
        {
            _logger.LogInformation($"Getting all PDF documents for user: {userId}");
            return await _pdfDocumentRepository.GetByUserIdAsync(userId);
        }

        /// <inheritdoc/>
        public async Task<bool> DeletePdfAsync(int id, string userId)
        {
            _logger.LogInformation($"Deleting PDF document with ID: {id} for user: {userId}");
            
            var document = await _pdfDocumentRepository.GetByIdAsync(id);
            
            if (document == null)
            {
                _logger.LogWarning($"Document with ID {id} not found");
                return false;
            }
            
            if (document.UploaderId != userId)
            {
                _logger.LogWarning($"User {userId} does not have permission to delete document {id}");
                return false;
            }
            
            try
            {
                // Delete the file from Google Cloud Storage
                var fileDeleted = await _googleStorageService.DeleteFileAsync(document.FilePath);
                if (!fileDeleted)
                {
                    _logger.LogWarning($"Failed to delete file from Google Cloud Storage: {document.FilePath}");
                }
                
                // Delete from database
                var result = await _pdfDocumentRepository.DeleteAsync(id, userId);
                _logger.LogInformation($"Document record deleted from database: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting PDF document: {ex.Message}");
                throw new Exception($"Failed to delete PDF document: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<PdfAccessToken> GenerateAccessTokenAsync(int pdfId, string userId, DateTime? expiresAt = null)
        {
            _logger.LogInformation($"Generating access token for document {pdfId} by user {userId}");
            
            try
            {
                var document = await _pdfDocumentRepository.GetByIdAsync(pdfId);
                
                if (document == null)
                {
                    _logger.LogWarning($"Document with ID {pdfId} not found");
                    throw new ArgumentException($"Document with ID {pdfId} not found.");
                }
                
                if (document.UploaderId != userId)
                {
                    _logger.LogWarning($"User {userId} does not have permission to share document {pdfId}");
                    throw new ArgumentException("You don't have permission to share this document.");
                }
                
                // Default expiration is 7 days
                if (!expiresAt.HasValue || expiresAt.Value <= DateTime.UtcNow)
                {
                    expiresAt = DateTime.UtcNow.AddDays(7);
                    _logger.LogInformation($"Using default expiration date: {expiresAt}");
                }
                
                var token = new PdfAccessToken
                {
                    DocumentId = pdfId,
                    Token = Guid.NewGuid(),
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    ExpiresAt = DateTime.SpecifyKind(expiresAt.Value, DateTimeKind.Unspecified),
                    IsRevoked = false
                };
                
                var result = await _pdfDocumentRepository.CreateAccessTokenAsync(token);
                _logger.LogInformation($"Access token created: {result.Token}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating access token: {ex.Message}");
                throw new Exception($"Failed to generate access token for document {pdfId}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<PdfDocument> GetPdfByTokenAsync(Guid token, string ipAddress = null, string userAgent = null)
        {
            _logger.LogInformation($"Getting PDF document by token: {token}");
            
            try
            {
                var document = await _pdfDocumentRepository.GetByTokenAsync(token);
                
                if (document == null)
                {
                    _logger.LogWarning($"No document found for token: {token}");
                    return null;
                }
                
                // Log the access
                await _pdfDocumentRepository.LogAccessAsync(new PdfAccessLog
                {
                    DocumentId = document.Id,
                    AccessedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });
                
                _logger.LogInformation($"Access logged for document {document.Id} with token {token}");
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting PDF by token: {ex.Message}");
                throw new Exception($"Failed to get PDF by token: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateTokenAsync(Guid token)
        {
            _logger.LogInformation($"Validating token: {token}");
            
            try
            {
                var accessToken = await _pdfDocumentRepository.GetAccessTokenAsync(token);
                
                if (accessToken == null)
                {
                    _logger.LogWarning($"Token not found: {token}");
                    return false;
                }
                
                var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                var isValid = accessToken.ExpiresAt > now && !accessToken.IsRevoked;
                
                _logger.LogInformation($"Token {token} is valid: {isValid}");
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating token: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeTokenAsync(Guid token, string userId)
        {
            _logger.LogInformation($"Revoking token {token} by user {userId}");
            
            try
            {
                var accessToken = await _pdfDocumentRepository.GetAccessTokenAsync(token);
                
                if (accessToken == null)
                {
                    _logger.LogWarning($"Token not found: {token}");
                    return false;
                }
                
                if (accessToken.Document.UploaderId != userId)
                {
                    _logger.LogWarning($"User {userId} does not have permission to revoke token {token}");
                    return false;
                }
                
                accessToken.IsRevoked = true;
                var result = await _pdfDocumentRepository.UpdateAccessTokenAsync(accessToken);
                
                _logger.LogInformation($"Token {token} revoked: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking token: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets the file content for a PDF document
        /// </summary>
        /// <param name="document">The PDF document</param>
        /// <returns>The file content as a byte array</returns>
        public async Task<byte[]> GetPdfContentAsync(PdfDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            
            _logger.LogInformation($"Getting PDF content for document {document.Id} from path {document.FilePath}");
            
            try
            {
                // Get the file from Google Cloud Storage
                var fileContent = await _googleStorageService.GetFileAsync(document.FilePath);
                _logger.LogInformation($"Retrieved PDF content, size: {fileContent.Length} bytes");
                return fileContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting PDF content: {ex.Message}");
                throw new Exception($"Failed to get PDF content: {ex.Message}", ex);
            }
        }
    }
}
