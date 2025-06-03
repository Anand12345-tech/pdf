using Swashbuckle.AspNetCore.Annotations;
using System;

namespace PdfManagement.Models.Documents
{
    public class ShareDocumentModel
    {
        [SwaggerSchema(Description = "Optional expiration date for the access token")]
        public DateTime? ExpiresAt { get; set; }
    }
}
