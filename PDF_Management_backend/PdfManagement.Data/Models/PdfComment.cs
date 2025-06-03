using System;
using System.Collections.Generic;

namespace PdfManagement.Data.Models
{
    public class PdfComment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string UserType { get; set; } = "user"; // "user" or "invited"
        public string? CommenterName { get; set; } // For guest users who provide their name
        
        // Foreign key for the document
        public int DocumentId { get; set; }
        
        // Foreign key for the commenter (can be null for anonymous/invited users)
        public string? CommenterId { get; set; }
        
        // Navigation properties
        public virtual PdfDocument PdfDocument { get; set; } = null!;
        public virtual ApplicationUser? Commenter { get; set; }
        
        // Self-referencing relationship for nested comments/replies
        public int? ParentCommentId { get; set; }
        public virtual PdfComment? ParentComment { get; set; }
        public virtual ICollection<PdfComment> Replies { get; set; } = new List<PdfComment>();
    }
}
