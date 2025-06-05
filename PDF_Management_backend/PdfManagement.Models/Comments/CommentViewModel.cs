using Swashbuckle.AspNetCore.Annotations;
using System;

namespace PdfManagement.Models.Comments
{
    public class CommentViewModel
    {
        [SwaggerSchema(Description = "Unique identifier for the comment")]
        public int Id { get; set; }

        [SwaggerSchema(Description = "Content of the comment")]
        public string Content { get; set; } = string.Empty;

        [SwaggerSchema(Description = "When the comment was created")]
        public DateTime CreatedAt { get; set; }

        [SwaggerSchema(Description = "When the comment was last updated")]
        public DateTime? UpdatedAt { get; set; }

        [SwaggerSchema(Description = "ID of the document this comment belongs to")]
        public int DocumentId { get; set; }

        [SwaggerSchema(Description = "ID of the user who created the comment")]
        public string UserId { get; set; } = string.Empty;

        [SwaggerSchema(Description = "Name of the user who created the comment")]
        public string UserName { get; set; } = string.Empty;
    }
}
