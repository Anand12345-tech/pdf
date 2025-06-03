using System;

namespace PdfManagement.Data.Models
{
    public class PdfAccessToken
    {
        public int Id { get; set; }
        public Guid Token { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Foreign key for the document
        public int PdfDocumentId { get; set; }
        
        // Navigation property
        public virtual PdfDocument PdfDocument { get; set; } = null!;
        
        // Access logs
        public virtual ICollection<PdfAccessLog> AccessLogs { get; set; } = new List<PdfAccessLog>();
    }
}
