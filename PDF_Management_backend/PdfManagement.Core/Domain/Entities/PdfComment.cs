using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace PdfManagement.Core.Domain.Entities
{
    /// <summary>
    /// PDF comment entity
    /// </summary>
    [Index(nameof(PageNumber))]
    public class PdfComment
    {
        public int Id { get; set; }
        
        [Required]
        public int DocumentId { get; set; }
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public int PageNumber { get; set; }
        
        public string? CommenterId { get; set; }
        
        [Required]
        public string UserType { get; set; } = "user"; // user, invited, etc.
        
        public string? CommenterName { get; set; } // For guest users who provide their name
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }
        
        public int? ParentCommentId { get; set; }
        
        // Navigation properties
        public virtual PdfDocument? Document { get; set; }
        public virtual ApplicationUser? Commenter { get; set; }
        public virtual PdfComment? ParentComment { get; set; }
        public virtual ICollection<PdfComment> Replies { get; set; } = new List<PdfComment>();
    }
}
