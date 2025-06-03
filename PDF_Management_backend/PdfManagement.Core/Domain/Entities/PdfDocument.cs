using System;
using System.Collections.Generic;

namespace PdfManagement.Core.Domain.Entities
{
    /// <summary>
    /// PDF document entity
    /// </summary>
    public class PdfDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = "application/pdf";
        public string UploaderId { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        
        // Navigation properties
        public virtual ApplicationUser? Uploader { get; set; }
        public virtual ICollection<PdfComment> Comments { get; set; } = new List<PdfComment>();
        public virtual ICollection<PdfAccessToken> AccessTokens { get; set; } = new List<PdfAccessToken>();
        public virtual ICollection<PdfAccessLog> AccessLogs { get; set; } = new List<PdfAccessLog>();
    }
}
