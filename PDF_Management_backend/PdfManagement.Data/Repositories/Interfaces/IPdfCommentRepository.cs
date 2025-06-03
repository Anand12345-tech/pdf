using PdfManagement.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository for PDF comment data operations
    /// </summary>
    public interface IPdfCommentRepository
    {
        /// <summary>
        /// Gets a comment by its ID
        /// </summary>
        /// <param name="id">The comment ID</param>
        /// <returns>The comment or null if not found</returns>
        Task<PdfComment> GetByIdAsync(int id);
        
        /// <summary>
        /// Gets all comments for a specific PDF document
        /// </summary>
        /// <param name="pdfId">The PDF document ID</param>
        /// <returns>Collection of comments</returns>
        Task<IEnumerable<PdfComment>> GetByPdfIdAsync(int pdfId);
        
        /// <summary>
        /// Gets all replies to a specific comment
        /// </summary>
        /// <param name="commentId">The parent comment ID</param>
        /// <returns>Collection of reply comments</returns>
        Task<IEnumerable<PdfComment>> GetRepliesAsync(int commentId);
        
        /// <summary>
        /// Adds a new comment
        /// </summary>
        /// <param name="comment">The comment to add</param>
        /// <returns>The added comment with its ID</returns>
        Task<PdfComment> AddAsync(PdfComment comment);
        
        /// <summary>
        /// Updates an existing comment
        /// </summary>
        /// <param name="comment">The comment to update</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateAsync(PdfComment comment);
        
        /// <summary>
        /// Deletes a comment
        /// </summary>
        /// <param name="id">The comment ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteAsync(int id);
    }
}
