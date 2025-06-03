using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PdfManagement.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string userId);
        Task<byte[]> GetFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
    }
}
