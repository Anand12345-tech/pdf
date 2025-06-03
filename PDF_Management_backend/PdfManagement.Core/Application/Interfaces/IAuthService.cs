using PdfManagement.Core.Domain.Entities;
using System.Threading.Tasks;

namespace PdfManagement.Core.Application.Interfaces
{
    /// <summary>
    /// Service interface for authentication operations
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <param name="firstName">User first name</param>
        /// <param name="lastName">User last name</param>
        /// <returns>Result of the registration with any errors</returns>
        Task<(bool Success, string[] Errors)> RegisterUserAsync(string email, string password, string firstName, string lastName);
        
        /// <summary>
        /// Authenticates a user and generates a JWT token
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="password">User password</param>
        /// <returns>JWT token if authentication is successful, null otherwise</returns>
        Task<string?> LoginUserAsync(string email, string password);
        
        /// <summary>
        /// Generates a JWT token for an authenticated user
        /// </summary>
        /// <param name="user">The authenticated user</param>
        /// <returns>JWT token</returns>
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
    }
}
