using System;
using System.Collections.Generic;

namespace PdfManagement.Data.Models
{
    public class PdfDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key for the uploader
        public string UploaderId { get; set; } = string.Empty;
        
        // Navigation property
        public virtual ApplicationUser Uploader { get; set; } = null!;
        
        // Access tokens for sharing
        public virtual ICollection<PdfAccessToken> AccessTokens { get; set; } = new List<PdfAccessToken>();
        
        // Comments on this document
        public virtual ICollection<PdfComment> Comments { get; set; } = new List<PdfComment>();
    }
}
