using System.ComponentModel.DataAnnotations;

namespace PdfManagement.API.Models.Comments
{
    /// <summary>
    /// Request model for updating a comment
    /// </summary>
    public class UpdateCommentRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
