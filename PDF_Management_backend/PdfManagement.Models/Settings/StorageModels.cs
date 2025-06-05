using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PdfManagement.Models.Settings
{
    public class StorageSettingsModel
    {
        [SwaggerSchema(Description = "Storage provider (Local)")]
        [Required]
        public string Provider { get; set; } = "Local";

        [SwaggerSchema(Description = "Local storage path")]
        public string? LocalBasePath { get; set; }
    }
}
