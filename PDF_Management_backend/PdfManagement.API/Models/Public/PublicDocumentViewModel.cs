using PdfManagement.API.Models.Comments;
using System.Collections.Generic;

namespace PdfManagement.API.Models.Public
{
    /// <summary>
    /// View model for public document access
    /// </summary>
    public class PublicDocumentViewModel
    {
        public DocumentInfo Document { get; set; } = new DocumentInfo();
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
        public string DownloadUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Basic document information
    /// </summary>
    public class DocumentInfo
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public System.DateTime UploadedAt { get; set; }
    }
}
