using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PdfManagement.Core.Application.Interfaces
{
    /// <summary>
    /// Service interface for file storage operations
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves a file to storage
        /// </summary>
        /// <param name="file">The file to save</param>
        /// <param name="fileName">The file name</param>
        /// <returns>The file path</returns>
        Task<string> SaveFileAsync(IFormFile file, string fileName);
        
        /// <summary>
        /// Gets a file from storage
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>The file bytes</returns>
        Task<byte[]> GetFileAsync(string filePath);
        
        /// <summary>
        /// Deletes a file from storage
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteFileAsync(string filePath);
    }
}
