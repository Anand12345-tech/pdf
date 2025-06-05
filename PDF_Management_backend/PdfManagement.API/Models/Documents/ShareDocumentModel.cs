using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PdfManagement.API.Models.Documents
{
    /// <summary>
    /// Model for sharing a document
    /// </summary>
    public class ShareDocumentModel
    {
        [JsonPropertyName("expiresAt")]
        public DateTime? ExpiresAt { get; set; }
    }
}
