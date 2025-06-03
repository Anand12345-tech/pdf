using PdfManagement.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Core.Application.Interfaces
{
    /// <summary>
    /// Service interface for PDF comment operations
    /// </summary>
    public interface IPdfCommentService
    {
        /// <summary>
        /// Adds a new comment to a PDF document
        /// </summary>
        /// <param name="pdfId">Document ID</param>
        /// <param name="content">Comment content</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="userId">User ID (optional for public comments)</param>
        /// <param name="userType">User type (user, invited, etc.)</param>
        /// <param name="parentCommentId">Parent comment ID (optional)</param>
        /// <param name="commenterName">Commenter name (optional, for guest users)</param>
        /// <returns>The added comment</returns>
        Task<PdfComment> AddCommentAsync(int pdfId, string content, int pageNumber, string? userId = null, string userType = "user", int? parentCommentId = null, string? commenterName = null);
        
        /// <summary>
        /// Gets all comments for a PDF document
        /// </summary>
        /// <param name="pdfId">Document ID</param>
        /// <returns>Collection of comments</returns>
        Task<IEnumerable<PdfComment>> GetCommentsForPdfAsync(int pdfId);
        
        /// <summary>
        /// Gets all replies to a comment
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <returns>Collection of reply comments</returns>
        Task<IEnumerable<PdfComment>> GetCommentRepliesAsync(int commentId);
        
        /// <summary>
        /// Deletes a comment
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <param name="userId">User ID for verification</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteCommentAsync(int commentId, string userId);
        
        /// <summary>
        /// Updates a comment
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <param name="content">New content</param>
        /// <param name="userId">User ID for verification</param>
        /// <returns>The updated comment or null if not found or not authorized</returns>
        Task<PdfComment?> UpdateCommentAsync(int commentId, string content, string userId);
    }
}
