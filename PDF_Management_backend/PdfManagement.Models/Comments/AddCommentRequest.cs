using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PdfManagement.Models.Comments
{
    public class AddCommentRequest
    {
        [Required]
        [SwaggerSchema(Description = "Content of the comment")]
        public string Content { get; set; } = string.Empty;
    }
}
