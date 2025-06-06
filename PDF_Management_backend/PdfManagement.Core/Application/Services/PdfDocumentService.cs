using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Http;
using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Core.Domain.Entities;
using PdfManagement.Core.Domain.Interfaces;
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
        private readonly IGoogleStorageService _googleStorageService;

        public PdfDocumentService(IPdfDocumentRepository pdfDocumentRepository, IGoogleStorageService googleStorageService)
        {
            _pdfDocumentRepository = pdfDocumentRepository;
            _googleStorageService = googleStorageService;

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
            //var filePath = await _fileStorageService.SaveFileAsync(file, uniqueFileName);
            var filepath = await _googleStorageService.SaveFileAsync(file,uniqueFileName);

            // Create document record
            var document = new PdfDocument
            {
                FileName = fileName,
                FilePath = filepath,
                FileSize = file.Length,
                ContentType = file.ContentType,
                UploaderId = userId,
                UploadedAt = DateTime.UtcNow
            };

            // Save to database
            return await _pdfDocumentRepository.AddAsync(document);
        }

        public async Task<PdfDocument> GetPdfByIdAsync(int id)
        {
            return await _pdfDocumentRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<PdfDocument>> GetUserPdfsAsync(string userId)
        {
            return await _pdfDocumentRepository.GetByUserIdAsync(userId);
        }

        public async Task<bool> DeletePdfAsync(int id, string userId)
        {
            var document = await _pdfDocumentRepository.GetByIdAsync(id);

            if (document == null || document.UploaderId != userId)
            {
                return false;
            }

            //await _fileStorageService.DeleteFileAsync(document.FilePath);
            await _googleStorageService.DeleteFileAsync(document.FilePath);

            return await _pdfDocumentRepository.DeleteAsync(id, userId);
        }

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
                    CreatedAt = DateTime.UtcNow, // Use UTC time directly - will be stored as timestamp with time zone
                    ExpiresAt = expiresAt.Value.ToUniversalTime(), // Ensure UTC time - will be stored as timestamp with time zone
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

        public async Task<PdfDocument> GetPdfByTokenAsync(Guid token, string ipAddress = null, string userAgent = null)
        {
            var document = await _pdfDocumentRepository.GetByTokenAsync(token);

            if (document != null)
            {
                // Log the access
                await _pdfDocumentRepository.LogAccessAsync(new PdfAccessLog
                {
                    DocumentId = document.Id,
                    AccessedAt = DateTime.UtcNow, // Use UTC time directly
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });
            }

            return document;
        }


        public async Task<bool> ValidateTokenAsync(Guid token)
        {
            var accessToken = await _pdfDocumentRepository.GetAccessTokenAsync(token);
            var now = DateTime.UtcNow; // Use UTC time directly

            return accessToken != null &&
                   accessToken.ExpiresAt > now &&
                   !accessToken.IsRevoked;
        }

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
