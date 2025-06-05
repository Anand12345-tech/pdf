using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Core.Domain.Entities;
using PdfManagement.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PdfManagement.Core.Application.Services
{
    /// <summary>
    /// Implementation of the authentication service
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;

        public AuthService(
            IAuthRepository authRepository,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<(bool Success, string[] Errors)> RegisterUserAsync(
           string email, 
            string password, 
            string firstName, 
            string lastName)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var (success, errors) = await _authRepository.CreateUserAsync(user, password);

            if (success)
            {
                // Add user to default role
                await _authRepository.AddUserToRoleAsync(user, "User");
                return (true, Array.Empty<string>());
            }

            return (false, errors.ToArray());
        }

        /// <inheritdoc/>
        public async Task<string?> LoginUserAsync(string email, string password)
        {
            var user = await _authRepository.FindUserByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            var isValid = await _authRepository.ValidateUserCredentialsAsync(user, password);
            if (!isValid)
            {
                return null;
            }

            return await GenerateJwtTokenAsync(user);
        }

        /// <inheritdoc/>
        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _authRepository.GetUserRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // First try to get JWT settings from environment variables, then fall back to configuration
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? _configuration["Jwt:Key"] ?? string.Empty;
            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"] ?? string.Empty;
            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"] ?? string.Empty;
            var jwtExpireDays = Environment.GetEnvironmentVariable("JWT_EXPIRE_DAYS") ?? _configuration["Jwt:ExpireDays"] ?? "7";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(jwtExpireDays));

            var token = new JwtSecurityToken(
                jwtIssuer,
                jwtAudience,
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
