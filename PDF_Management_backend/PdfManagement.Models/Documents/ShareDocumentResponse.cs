using Swashbuckle.AspNetCore.Annotations;
using System;

namespace PdfManagement.Models.Documents
{
    public class ShareDocumentResponse
    {
        [SwaggerSchema(Description = "Unique token for accessing the document")]
        public Guid Token { get; set; }

        [SwaggerSchema(Description = "When the access token expires")]
        public DateTime? ExpiresAt { get; set; }

        [SwaggerSchema(Description = "URL to access the document")]
        public string Url { get; set; } = string.Empty;
    }
}
