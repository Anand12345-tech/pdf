using Swashbuckle.AspNetCore.Annotations;
using System;

namespace PdfManagement.Models.Documents
{
    public class DocumentViewModel
    {
        [SwaggerSchema(Description = "Unique identifier for the document")]
        public int Id { get; set; }

        [SwaggerSchema(Description = "Original filename of the document")]
        public string FileName { get; set; } = string.Empty;

        [SwaggerSchema(Description = "Size of the file in bytes")]
        public long FileSize { get; set; }

        [SwaggerSchema(Description = "When the document was uploaded")]
        public DateTime UploadedAt { get; set; }

        [SwaggerSchema(Description = "URL to download the document")]
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
