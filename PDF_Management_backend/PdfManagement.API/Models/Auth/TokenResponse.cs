namespace PdfManagement.API.Models.Auth
{
    /// <summary>
    /// Response model for authentication token
    /// </summary>
    public class TokenResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}
