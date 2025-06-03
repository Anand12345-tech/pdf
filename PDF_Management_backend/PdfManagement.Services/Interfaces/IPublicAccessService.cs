using PdfManagement.Data.Models;
using PdfManagement.Models.Comments;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Services.Interfaces
{
    /// <summary>
    /// Service for handling public access to documents
    /// </summary>
    public interface IPublicAccessService
    {
        /// <summary>
        /// Gets a document by its access token
        /// </summary>
        /// <param name="token">The access token</param>
        /// <param name="ipAddress">The IP address of the requester</param>
        /// <param name="userAgent">The user agent of the requester</param>
        /// <returns>The document if the token is valid, null otherwise</returns>
        Task<PdfDocument> GetDocumentByTokenAsync(Guid token, string ipAddress = null, string userAgent = null);
        
        /// <summary>
        /// Gets comments for a document accessed via token
        /// </summary>
        /// <param name="token">The access token</param>
        /// <returns>Collection of comments if the token is valid, null otherwise</returns>
        Task<IEnumerable<PdfComment>> GetCommentsForTokenAsync(Guid token);
        
        /// <summary>
        /// Adds a comment to a document accessed via token
        /// </summary>
        /// <param name="token">The access token</param>
        /// <param name="content">The comment content</param>
        /// <param name="pageNumber">The page number</param>
        /// <param name="parentCommentId">Optional parent comment ID</param>
        /// <returns>The added comment if successful, null otherwise</returns>
        Task<PdfComment> AddCommentToTokenDocumentAsync(
            Guid token, 
            string content, 
            int pageNumber, 
            int? parentCommentId = null);
    }
}
