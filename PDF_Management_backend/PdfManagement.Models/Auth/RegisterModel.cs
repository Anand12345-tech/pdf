using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace PdfManagement.Models.Auth
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        [SwaggerSchema(Description = "User's email address")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [SwaggerSchema(Description = "User's password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [SwaggerSchema(Description = "User's first name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [SwaggerSchema(Description = "User's last name")]
        public string LastName { get; set; } = string.Empty;
    }
}
