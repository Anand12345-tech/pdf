using PdfManagement.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Core.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for PDF comment operations
    /// </summary>
    public interface IPdfCommentRepository : IRepository<PdfComment>
    {
        /// <summary>
        /// Gets all comments for a specific PDF document
        /// </summary>
        /// <param name="pdfId">PDF document ID</param>
        /// <returns>Collection of comments</returns>
        Task<IEnumerable<PdfComment>> GetByPdfIdAsync(int pdfId);
        
        /// <summary>
        /// Gets all replies to a specific comment
        /// </summary>
        /// <param name="commentId">Parent comment ID</param>
        /// <returns>Collection of reply comments</returns>
        Task<IEnumerable<PdfComment>> GetRepliesAsync(int commentId);
        
        /// <summary>
        /// Deletes a comment with all its replies
        /// </summary>
        /// <param name="id">Comment ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteWithRepliesAsync(int id);
    }
}
