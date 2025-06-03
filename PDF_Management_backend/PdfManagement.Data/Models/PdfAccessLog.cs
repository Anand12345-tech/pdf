using System;

namespace PdfManagement.Data.Models
{
    public class PdfAccessLog
    {
        public int Id { get; set; }
        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        
        // Foreign key for the access token
        public int AccessTokenId { get; set; }
        
        // Navigation property
        public virtual PdfAccessToken AccessToken { get; set; } = null!;
    }
}
