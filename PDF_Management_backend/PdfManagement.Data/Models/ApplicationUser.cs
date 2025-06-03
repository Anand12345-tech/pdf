using Microsoft.AspNetCore.Identity;

namespace PdfManagement.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Additional user properties can be added here
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<PdfDocument> UploadedDocuments { get; set; } = new List<PdfDocument>();
        public virtual ICollection<PdfComment> Comments { get; set; } = new List<PdfComment>();
    }
}
