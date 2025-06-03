using Microsoft.AspNetCore.Http;
using PdfManagement.Data.Models;
using System.Threading.Tasks;

namespace PdfManagement.Services.Interfaces
{
    public interface IPdfDocumentService
    {
        Task<PdfDocument> UploadPdfAsync(IFormFile file, string userId);
        Task<PdfDocument?> GetPdfByIdAsync(int id);
        Task<IEnumerable<PdfDocument>> GetUserPdfsAsync(string userId);
        Task<bool> DeletePdfAsync(int id, string userId);
        Task<PdfAccessToken> GenerateAccessTokenAsync(int pdfId, string userId, DateTime? expiresAt = null);
        Task<PdfDocument?> GetPdfByTokenAsync(Guid token, string? ipAddress = null, string? userAgent = null);
        Task<bool> ValidateTokenAsync(Guid token);
        Task<bool> RevokeTokenAsync(Guid token, string userId);
    }
}
