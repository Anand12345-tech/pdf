using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;

namespace PdfManagement.Models.Comments
{
    public class CommentViewModel
    {
        [SwaggerSchema(Description = "Unique identifier for the comment")]
        public int Id { get; set; }

        [SwaggerSchema(Description = "Content of the comment")]
        public string Content { get; set; } = string.Empty;

        [SwaggerSchema(Description = "Page number where the comment is placed")]
        public int PageNumber { get; set; }

        [SwaggerSchema(Description = "When the comment was created")]
        public DateTime CreatedAt { get; set; }

        [SwaggerSchema(Description = "Type of user who created the comment (user/invited)")]
        public string UserType { get; set; } = string.Empty;

        [SwaggerSchema(Description = "ID of the user who created the comment (null for anonymous users)")]
        public string? CommenterId { get; set; }

        [SwaggerSchema(Description = "Username of the commenter")]
        public string? CommenterName { get; set; }

        [SwaggerSchema(Description = "ID of the parent comment if this is a reply")]
        public int? ParentCommentId { get; set; }

        [SwaggerSchema(Description = "Replies to this comment")]
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
    }
}
