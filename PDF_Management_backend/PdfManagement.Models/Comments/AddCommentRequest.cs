using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PdfManagement.Models.Comments
{
    public class AddCommentRequest
    {
        [Required]
        [SwaggerSchema(Description = "Content of the comment")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [SwaggerSchema(Description = "Page number where the comment is placed")]
        public int PageNumber { get; set; }

        [SwaggerSchema(Description = "ID of the parent comment if this is a reply")]
        public int? ParentCommentId { get; set; }
    }
}
