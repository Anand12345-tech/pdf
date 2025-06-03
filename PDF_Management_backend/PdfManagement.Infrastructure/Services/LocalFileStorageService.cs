using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PdfManagement.Core.Application.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PdfManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the file storage service using local file system
    /// </summary>
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _uploadsFolder;

        public LocalFileStorageService()
        {
            // Use a relative path for uploads
            _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            
            // Ensure uploads directory exists
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder);
            }
        }

        /// <inheritdoc/>
        public async Task<string> SaveFileAsync(IFormFile file, string fileName)
        {
            var filePath = Path.Combine(_uploadsFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            return filePath;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }
            
            return await File.ReadAllBytesAsync(filePath);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }
    }
}
