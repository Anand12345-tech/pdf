using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PdfManagement.API.Models.Auth;
using PdfManagement.API.Models.Common;
using PdfManagement.Core.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace PdfManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [SwaggerResponse(StatusCodes.Status200OK, "User registered successfully", typeof(ApiResponse))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input or validation errors", typeof(ValidationProblemDetails))]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, errors) = await _authService.RegisterUserAsync( model.Email,  model.Password,  model.FirstName, model.LastName);

            if (!success)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return BadRequest(ModelState);
            }

            return Ok(new ApiResponse { Success = true, Message = "User registered successfully" });
        }

        [HttpPost("login")]
        [SwaggerResponse(StatusCodes.Status200OK, "Login successful", typeof(ApiResponse<TokenResponse>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid credentials")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input", typeof(ValidationProblemDetails))]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = await _authService.LoginUserAsync(model.Email, model.Password);
            
            if (token == null)
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "Invalid email or password" });
            }

            return Ok(new ApiResponse<TokenResponse> 
            { 
                Success = true, 
                Message = "Login successful", 
                Data = new TokenResponse { Token = token } 
            });
        }
    }
}
