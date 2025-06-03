using PdfManagement.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Core.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for PDF document operations
    /// </summary>
    public interface IPdfDocumentRepository : IRepository<PdfDocument>
    {
        /// <summary>
        /// Gets all PDF documents for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Collection of PDF documents</returns>
        Task<IEnumerable<PdfDocument>> GetByUserIdAsync(string userId);
        
        /// <summary>
        /// Deletes a PDF document with verification of ownership
        /// </summary>
        /// <param name="id">Document ID</param>
        /// <param name="userId">User ID for verification</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteWithUserVerificationAsync(int id, string userId);
        
        /// <summary>
        /// Creates an access token for a PDF document
        /// </summary>
        /// <param name="token">Access token entity</param>
        /// <returns>Created access token</returns>
        Task<PdfAccessToken> CreateAccessTokenAsync(PdfAccessToken token);
        
        /// <summary>
        /// Gets a PDF document by an access token
        /// </summary>
        /// <param name="token">Token value</param>
        /// <returns>PDF document or null if not found or token expired</returns>
        Task<PdfDocument?> GetByTokenAsync(Guid token);
        
        /// <summary>
        /// Gets an access token by its value
        /// </summary>
        /// <param name="token">Token value</param>
        /// <returns>Access token or null if not found</returns>
        Task<PdfAccessToken?> GetAccessTokenAsync(Guid token);
        
        /// <summary>
        /// Updates an access token
        /// </summary>
        /// <param name="token">Token to update</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateAccessTokenAsync(PdfAccessToken token);
        
        /// <summary>
        /// Logs an access to a PDF document
        /// </summary>
        /// <param name="log">Access log entry</param>
        /// <returns>Created log entry</returns>
        Task<PdfAccessLog> LogAccessAsync(PdfAccessLog log);
    }
}
