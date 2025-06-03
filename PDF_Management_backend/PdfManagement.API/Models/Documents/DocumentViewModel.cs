using System;

namespace PdfManagement.API.Models.Documents
{
    /// <summary>
    /// View model for PDF document
    /// </summary>
    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string ViewUrl { get; set; }
    }
}
