using System.ComponentModel.DataAnnotations;

namespace PdfManagement.API.Models.Comments
{
    /// <summary>
    /// Request model for adding a comment
    /// </summary>
    public class AddCommentRequest
    {
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; }
        
        public int? ParentCommentId { get; set; }
        
        public string? CommenterName { get; set; }
    }
}
