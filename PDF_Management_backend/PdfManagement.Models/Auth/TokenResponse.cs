using Swashbuckle.AspNetCore.Annotations;

namespace PdfManagement.Models.Auth
{
    public class TokenResponse
    {
        [SwaggerSchema(Description = "JWT authentication token")]
        public string Token { get; set; } = string.Empty;
    }
}
