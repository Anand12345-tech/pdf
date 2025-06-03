using Microsoft.AspNetCore.Identity;

namespace PdfManagement.Core.Domain.Entities
{
    /// <summary>
    /// Application user entity extending Identity user
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        
        // Navigation properties
        public virtual ICollection<PdfDocument> Documents { get; set; } = new List<PdfDocument>();
        public virtual ICollection<PdfComment> Comments { get; set; } = new List<PdfComment>();
    }
}
