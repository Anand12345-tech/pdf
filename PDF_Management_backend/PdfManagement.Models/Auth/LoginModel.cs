using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PdfManagement.Models.Auth
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        [SwaggerSchema(Description = "User's email address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [SwaggerSchema(Description = "User's password")]
        public string Password { get; set; } = string.Empty;
    }
}
