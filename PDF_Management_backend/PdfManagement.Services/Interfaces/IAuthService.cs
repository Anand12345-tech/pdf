using PdfManagement.Data.Models;
using PdfManagement.Models.Auth;
using System.Threading.Tasks;

namespace PdfManagement.Services.Interfaces
{
    /// <summary>
    /// Service for handling authentication and user management operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="model">Registration details</param>
        /// <returns>Result of the registration operation</returns>
        Task<(bool Success, string[] Errors)> RegisterUserAsync(RegisterModel model);
        
        /// <summary>
        /// Authenticates a user and generates a JWT token
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>JWT token if authentication is successful, null otherwise</returns>
        Task<string> LoginUserAsync(LoginModel model);
        
        /// <summary>
        /// Generates a JWT token for an authenticated user
        /// </summary>
        /// <param name="user">The authenticated user</param>
        /// <returns>JWT token</returns>
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    }
}
