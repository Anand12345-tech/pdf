using Microsoft.AspNetCore.Http;
using PdfManagement.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Core.Application.Interfaces
{
    /// <summary>
    /// Service interface for PDF document operations
    /// </summary>
    public interface IPdfDocumentService
    {
        /// <summary>
        /// Uploads a new PDF document
        /// </summary>
        /// <param name="file">The PDF file</param>
        /// <param name="userId">User ID of the uploader</param>
        /// <returns>The uploaded document</returns>
        Task<PdfDocument> UploadPdfAsync(IFormFile file, string userId);
        
        /// <summary>
        /// Gets a PDF document by its ID
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <returns>The document or null if not found</returns>
        Task<PdfDocument?> GetPdfByIdAsync(int id);
        
        /// <summary>
        /// Gets all PDF documents for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Collection of documents</returns>
        Task<IEnumerable<PdfDocument>> GetUserPdfsAsync(string userId);
        
        /// <summary>
        /// Deletes a PDF document
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="userId">User ID for verification</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeletePdfAsync(int id, string userId);
        
        /// <summary>
        /// Generates an access token for sharing a document
        /// </summary>
        /// <param name="pdfId">Document ID</param>
        /// <param name="userId">User ID for verification</param>
        /// <param name="expiresAt">Optional expiration date</param>
        /// <returns>The generated access token</returns>
        Task<PdfAccessToken> GenerateAccessTokenAsync(int pdfId, string userId, DateTime? expiresAt = null);
        
        /// <summary>
        /// Gets a PDF document by an access token
        /// </summary>
        /// <param name="token">Token value</param>
        /// <param name="ipAddress">IP address of the requester</param>
        /// <param name="userAgent">User agent of the requester</param>
        /// <returns>The document or null if not found or token expired</returns>
        Task<PdfDocument?> GetPdfByTokenAsync(Guid token, string? ipAddress = null, string? userAgent = null);
        
        /// <summary>
        /// Validates an access token
        /// </summary>
        /// <param name="token">Token value</param>
        /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateTokenAsync(Guid token);
        
        /// <summary>
        /// Revokes an access token
        /// </summary>
        /// <param name="token">Token value</param>
        /// <param name="userId">User ID for verification</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RevokeTokenAsync(Guid token, string userId);
    }
}
