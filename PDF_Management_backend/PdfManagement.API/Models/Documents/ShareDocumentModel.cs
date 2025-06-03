using System;
using System.ComponentModel.DataAnnotations;

namespace PdfManagement.API.Models.Documents
{
    /// <summary>
    /// Model for sharing a document
    /// </summary>
    public class ShareDocumentModel
    {
        [Required]
        public DateTime? ExpiresAt { get; set; }
    }
}
