using System;

namespace PdfManagement.API.Models.Documents
{
    /// <summary>
    /// Response model for document sharing
    /// </summary>
    public class ShareDocumentResponse
    {
        public Guid Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
