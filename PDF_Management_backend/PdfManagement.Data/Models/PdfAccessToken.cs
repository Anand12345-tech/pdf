using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PdfManagement.Data.Models
{
    public class PdfAccessToken
    {
        public int Id { get; set; }
        public Guid Token { get; set; } = Guid.NewGuid();
        
        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Column(TypeName = "timestamp with time zone")]
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
