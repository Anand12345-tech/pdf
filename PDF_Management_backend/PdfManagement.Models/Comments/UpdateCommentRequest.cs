using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PdfManagement.Models.Comments
{
    public class UpdateCommentRequest
    {
        [Required]
        [SwaggerSchema(Description = "Updated content of the comment")]
        public string Content { get; set; } = string.Empty;
    }
}
