using System;
using System.ComponentModel.DataAnnotations.Schema;

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
        
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; }
        
        [Column(TypeName = "timestamp with time zone")]
        public DateTime ExpiresAt { get; set; }
        
        public bool IsRevoked { get; set; }
        
        // Navigation property
        public virtual PdfDocument? Document { get; set; }
    }
}
