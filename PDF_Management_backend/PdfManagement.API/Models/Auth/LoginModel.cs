using System.ComponentModel.DataAnnotations;

namespace PdfManagement.API.Models.Auth
{
    /// <summary>
    /// Model for user login
    /// </summary>
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
