using PdfManagement.Core.Domain.Entities;
using System.Threading.Tasks;

namespace PdfManagement.Services.Interfaces
{
    public interface IPdfCommentService
    {
        Task<PdfComment> AddCommentAsync(int pdfId, string content, int pageNumber, string? userId = null, string userType = "user", int? parentCommentId = null, string? commenterName = null);
        Task<IEnumerable<PdfComment>> GetCommentsForPdfAsync(int pdfId);
        Task<IEnumerable<PdfComment>> GetCommentRepliesAsync(int commentId);
        Task<bool> DeleteCommentAsync(int commentId, string userId);
        Task<PdfComment?> UpdateCommentAsync(int commentId, string content, string userId);
    }
}