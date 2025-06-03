using PdfManagement.Core.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Core.Domain.Interfaces
{
    /// <summary>
    /// Repository interface for authentication operations
    /// </summary>
    public interface IAuthRepository
    {
        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <param name="user">User entity</param>
        /// <param name="password">User password</param>
        /// <returns>Result of the operation with any errors</returns>
        Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(ApplicationUser user, string password);
        
        /// <summary>
        /// Finds a user by email
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>User entity or null if not found</returns>
        Task<ApplicationUser?> FindUserByEmailAsync(string email);
        
        /// <summary>
        /// Validates user credentials
        /// </summary>
        /// <param name="user">User entity</param>
        /// <param name="password">User password</param>
        /// <returns>True if valid, false otherwise</returns>
        Task<bool> ValidateUserCredentialsAsync(ApplicationUser user, string password);
        
        /// <summary>
        /// Gets user roles
        /// </summary>
        /// <param name="user">User entity</param>
        /// <returns>Collection of role names</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(ApplicationUser user);
        
        /// <summary>
        /// Adds a user to a role
        /// </summary>
        /// <param name="user">User entity</param>
        /// <param name="role">Role name</param>
        /// <returns>Result of the operation</returns>
        Task<bool> AddUserToRoleAsync(ApplicationUser user, string role);
    }
}
