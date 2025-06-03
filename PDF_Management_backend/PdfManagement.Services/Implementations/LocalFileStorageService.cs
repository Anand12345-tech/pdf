using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PdfManagement.Services.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PdfManagement.Services.Implementations
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _storageBasePath;

        public LocalFileStorageService(IConfiguration configuration)
        {
            _storageBasePath = configuration["FileStorage:LocalBasePath"] ?? 
                Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");
            
            // Ensure the storage directory exists
            if (!Directory.Exists(_storageBasePath))
            {
                Directory.CreateDirectory(_storageBasePath);
            }
        }

        public async Task<string> SaveFileAsync(IFormFile file, string userId)
        {
            // Create user directory if it doesn't exist
            var userDirectory = Path.Combine(_storageBasePath, userId);
            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            // Generate a unique filename
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(userDirectory, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return the relative path for storage in the database
            return Path.Combine(userId, fileName);
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_storageBasePath, filePath);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The requested file was not found.", fullPath);
            }

            return await File.ReadAllBytesAsync(fullPath);
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            var fullPath = Path.Combine(_storageBasePath, filePath);
            
            if (!File.Exists(fullPath))
            {
                return Task.FromResult(false);
            }

            File.Delete(fullPath);
            return Task.FromResult(true);
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = Path.Combine(_storageBasePath, filePath);
            return Task.FromResult(File.Exists(fullPath));
        }
    }
}
