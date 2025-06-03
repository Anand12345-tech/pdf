using System;
using System.Collections.Generic;

namespace PdfManagement.API.Models.Comments
{
    /// <summary>
    /// View model for PDF comment
    /// </summary>
    public class CommentViewModel
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserType { get; set; } = string.Empty;
        public string? CommenterId { get; set; }
        public string? CommenterName { get; set; }
        public int? ParentCommentId { get; set; }
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
    }
}
