using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PdfManagement.Core.Application.Interfaces
{
    public interface IGoogleStorageService
    {
        /// <summary>
        /// Saves a file to Google Cloud Storage
        /// </summary>
        /// <param name="file">The file to save</param>
        /// <param name="userId">The user ID to associate with the file</param>
        /// <returns>The file ID or path that can be used to retrieve the file later</returns>
        Task<string> SaveFileAsync(IFormFile file, string userId);
        
        /// <summary>
        /// Gets a file from Google Cloud Storage
        /// </summary>
        /// <param name="fileId">The file ID or path to retrieve</param>
        /// <returns>The file contents as a byte array</returns>
        Task<byte[]> GetFileAsync(string fileId);
        
        /// <summary>
        /// Deletes a file from Google Cloud Storage
        /// </summary>
        /// <param name="fileId">The file ID or path to delete</param>
        /// <returns>True if the file was deleted, false otherwise</returns>
        Task<bool> DeleteFileAsync(string fileId);
        
        /// <summary>
        /// Checks if a file exists in Google Cloud Storage
        /// </summary>
        /// <param name="fileId">The file ID or path to check</param>
        /// <returns>True if the file exists, false otherwise</returns>
        Task<bool> FileExistsAsync(string fileId);
    }
}
