using Swashbuckle.AspNetCore.Annotations;
using System;

namespace PdfManagement.Models.Documents
{
    public class JwtShareResponse
    {
        [SwaggerSchema(Description = "URL to access the shared document")]
        public string ShareUrl { get; set; } = string.Empty;

        [SwaggerSchema(Description = "JWT token for document access")]
        public string Token { get; set; } = string.Empty;

        [SwaggerSchema(Description = "When the token will expire")]
        public DateTime ExpiresAt { get; set; }

        [SwaggerSchema(Description = "ID of the shared document")]
        public int DocumentId { get; set; }

        [SwaggerSchema(Description = "Name of the shared document")]
        public string DocumentName { get; set; } = string.Empty;
    }
}
