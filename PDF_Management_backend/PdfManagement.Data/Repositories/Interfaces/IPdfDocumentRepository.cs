using PdfManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository for PDF document data operations
    /// </summary>
    public interface IPdfDocumentRepository
    {
        /// <summary>
        /// Gets a PDF document by its ID
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <returns>The PDF document or null if not found</returns>
        Task<PdfDocument> GetByIdAsync(int id);
        
        /// <summary>
        /// Gets all PDF documents for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>Collection of PDF documents</returns>
        Task<IEnumerable<PdfDocument>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Adds a new PDF document
        /// </summary>
        /// <param name="document">The document to add</param>
        /// <returns>The added document with its ID</returns>
        Task<PdfDocument> AddAsync(PdfDocument document);
        
        /// <summary>
        /// Updates an existing PDF document
        /// </summary>
        /// <param name="document">The document to update</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateAsync(PdfDocument document);
        
        /// <summary>
        /// Deletes a PDF document
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <param name="userId">The user ID (for verification)</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteAsync(int id, string userId);
        
        /// <summary>
        /// Creates an access token for a PDF document
        /// </summary>
        /// <param name="token">The access token to create</param>
        /// <returns>The created access token</returns>
        Task<PdfAccessToken> CreateAccessTokenAsync(PdfAccessToken token);
        
        /// <summary>
        /// Gets a PDF document by an access token
        /// </summary>
        /// <param name="token">The access token</param>
        /// <returns>The PDF document or null if not found or token expired</returns>
        Task<PdfDocument> GetByTokenAsync(Guid token);
        
        /// <summary>
        /// Gets an access token by its value
        /// </summary>
        /// <param name="token">The token value</param>
        /// <returns>The access token or null if not found</returns>
        Task<PdfAccessToken> GetAccessTokenAsync(Guid token);
        
        /// <summary>
        /// Updates an access token
        /// </summary>
        /// <param name="token">The token to update</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateAccessTokenAsync(PdfAccessToken token);
        
        /// <summary>
        /// Logs an access to a PDF document
        /// </summary>
        /// <param name="log">The access log entry</param>
        /// <returns>The created log entry</returns>
        Task<PdfAccessLog> LogAccessAsync(PdfAccessLog log);
    }
}
