using System;

namespace PdfManagement.Core.Domain.Entities
{
    /// <summary>
    /// PDF access token entity for sharing documents
    /// </summary>
    public class PdfAccessToken
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public Guid Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        
        // Navigation property
        public virtual PdfDocument? Document { get; set; }
    }
}
