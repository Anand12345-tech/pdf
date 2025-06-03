using System;

namespace PdfManagement.Core.Domain.Entities
{
    /// <summary>
    /// PDF access log entity for tracking document access
    /// </summary>
    public class PdfAccessLog
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public DateTime AccessedAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        
        // Navigation property
        public virtual PdfDocument? Document { get; set; }
    }
}
