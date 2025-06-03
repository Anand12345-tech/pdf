using PdfManagement.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Core.Application.Interfaces
{
    /// <summary>
    /// Service interface for public access to documents
    /// </summary>
    public interface IPublicAccessService
    {
        /// <summary>
        /// Gets a document by its access token
        /// </summary>
        /// <param name="token">Access token</param>
        /// <param name="ipAddress">IP address of the requester</param>
        /// <param name="userAgent">User agent of the requester</param>
        /// <returns>The document if the token is valid, null otherwise</returns>
        Task<PdfDocument?> GetDocumentByTokenAsync(Guid token, string? ipAddress = null, string? userAgent = null);
        
        /// <summary>
        /// Gets comments for a document accessed via token
        /// </summary>
        /// <param name="token">Access token</param>
        /// <returns>Collection of comments if the token is valid, null otherwise</returns>
        Task<IEnumerable<PdfComment>?> GetCommentsForTokenAsync(Guid token);
        
        /// <summary>
        /// Adds a comment to a document accessed via token
        /// </summary>
        /// <param name="token">Access token</param>
        /// <param name="content">Comment content</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="parentCommentId">Optional parent comment ID</param>
        /// <param name="commenterName">Optional commenter name for guest users</param>
        /// <returns>The added comment if successful, null otherwise</returns>
        Task<PdfComment?> AddCommentToTokenDocumentAsync(
            Guid token, 
            string content, 
            int pageNumber, 
            int? parentCommentId = null,
            string? commenterName = null);
    }
}
