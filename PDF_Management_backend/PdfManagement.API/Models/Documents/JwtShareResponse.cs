using System;

namespace PdfManagement.API.Models.Documents
{
    public class JwtShareResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string ShareUrl { get; set; }
    }
}
