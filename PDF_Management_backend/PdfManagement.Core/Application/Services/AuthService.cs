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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"] ?? "7"));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
