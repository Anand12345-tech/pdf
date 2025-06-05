using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PdfManagement.Models.Common;
using PdfManagement.Models.Settings;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PdfManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get storage settings
        /// </summary>
        /// <returns>Current storage settings</returns>
        [HttpGet("storage")]
        public IActionResult GetStorageSettings()
        {
            try
            {
                var settings = new StorageSettingsModel
                {
                    Provider = Environment.GetEnvironmentVariable("FILE_STORAGE_PROVIDER") ?? 
                              _configuration["FileStorage:Provider"] ?? 
                              "Local",
                    LocalBasePath = _configuration["FileStorage:LocalBasePath"]
                };

                return Ok(new ApiResponse<StorageSettingsModel> 
                { 
                    Success = true, 
                    Data = settings 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse 
                { 
                    Success = false, 
                    Message = $"Error retrieving storage settings: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Update storage settings
        /// </summary>
        /// <param name="settings">New storage settings</param>
        /// <returns>Updated storage settings</returns>
        [HttpPut("storage")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStorageSettings([FromBody] StorageSettingsModel settings)
        {
            try
            {
                // This is a simplified implementation
                // In a real application, you would update the configuration in a persistent store
                
                // For demonstration, we'll just write to a settings file
                var settingsJson = System.Text.Json.JsonSerializer.Serialize(settings, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                var settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "storage-settings.json");
                await System.IO.File.WriteAllTextAsync(settingsPath, settingsJson);
                
                return Ok(new ApiResponse<StorageSettingsModel> 
                { 
                    Success = true, 
                    Message = "Storage settings updated successfully",
                    Data = settings 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse 
                { 
                    Success = false, 
                    Message = $"Error updating storage settings: {ex.Message}" 
                });
            }
        }
    }
}
