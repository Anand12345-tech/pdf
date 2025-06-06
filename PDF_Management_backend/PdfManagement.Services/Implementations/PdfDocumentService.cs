using Microsoft.AspNetCore.Http;
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
    /// Implementation of the PDF document service
    /// </summary>
    public class PdfDocumentService : IPdfDocumentService
    {
        private readonly IPdfDocumentRepository _pdfDocumentRepository;
        private readonly IFileStorageService _fileStorageService;

        public PdfDocumentService(
            IPdfDocumentRepository pdfDocumentRepository,
            IFileStorageService fileStorageService)
        {
            _pdfDocumentRepository = pdfDocumentRepository;
            _fileStorageService = fileStorageService;
        }

        /// <inheritdoc/>
        public async Task<PdfDocument> UploadPdfAsync(IFormFile file, string userId)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file uploaded.");
            }

            if (file.ContentType != "application/pdf")
            {
                throw new ArgumentException("Only PDF files are allowed.");
            }

            // Generate a unique file name
            var fileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            
            // Save the file
            var filePath = await _fileStorageService.SaveFileAsync(file, uniqueFileName);
            
            // Create document record
            var document = new PdfDocument
            {
                FileName = fileName,
                FilePath = filePath,
                FileSize = file.Length,
                ContentType = file.ContentType,
                UploaderId = userId,
                UploadedAt = DateTime.UtcNow
            };
            
            // Save to database
            return await _pdfDocumentRepository.AddAsync(document);
        }

        /// <inheritdoc/>
        public async Task<PdfDocument> GetPdfByIdAsync(int id)
        {
            return await _pdfDocumentRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PdfDocument>> GetUserPdfsAsync(string userId)
        {
            return await _pdfDocumentRepository.GetByUserIdAsync(userId);
        }

        /// <inheritdoc/>
        public async Task<bool> DeletePdfAsync(int id, string userId)
        {
            var document = await _pdfDocumentRepository.GetByIdAsync(id);
            
            if (document == null || document.UploaderId != userId)
            {
                return false;
            }
            
            // Delete the file
            await _fileStorageService.DeleteFileAsync(document.FilePath);
            
            // Delete from database
            return await _pdfDocumentRepository.DeleteAsync(id, userId);
        }

        /// <inheritdoc/>
        public async Task<PdfAccessToken> GenerateAccessTokenAsync(int pdfId, string userId, DateTime? expiresAt = null)
        {
            try
            {
                var document = await _pdfDocumentRepository.GetByIdAsync(pdfId);
                
                if (document == null || document.UploaderId != userId)
                {
                    throw new ArgumentException("Document not found or you don't have permission to share it.");
                }
                
                // Default expiration is 7 days
                if (!expiresAt.HasValue || expiresAt.Value <= DateTime.UtcNow)
                {
                    expiresAt = DateTime.UtcNow.AddDays(7);
                }
                
                var token = new PdfAccessToken
                {
                    DocumentId = pdfId,
                    Token = Guid.NewGuid(),
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    ExpiresAt = DateTime.SpecifyKind(expiresAt.Value, DateTimeKind.Unspecified),
                    IsRevoked = false
                };
                
                return await _pdfDocumentRepository.CreateAccessTokenAsync(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GenerateAccessTokenAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw new Exception($"Failed to generate access token for document {pdfId}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<PdfDocument> GetPdfByTokenAsync(Guid token, string ipAddress = null, string userAgent = null)
        {
            var document = await _pdfDocumentRepository.GetByTokenAsync(token);
            
            if (document != null)
            {
                // Log the access
                await _pdfDocumentRepository.LogAccessAsync(new PdfAccessLog
                {
                    DocumentId = document.Id,
                    AccessedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });
            }
            
            return document;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateTokenAsync(Guid token)
        {
            var accessToken = await _pdfDocumentRepository.GetAccessTokenAsync(token);
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            
            return accessToken != null && 
                   accessToken.ExpiresAt > now && 
                   !accessToken.IsRevoked;
        }

        /// <inheritdoc/>
        public async Task<bool> RevokeTokenAsync(Guid token, string userId)
        {
            var accessToken = await _pdfDocumentRepository.GetAccessTokenAsync(token);
            
            if (accessToken == null || accessToken.Document.UploaderId != userId)
            {
                return false;
            }
            
            accessToken.IsRevoked = true;
            return await _pdfDocumentRepository.UpdateAccessTokenAsync(accessToken);
        }
    }
}
