using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PdfManagement.API.Models.Common;
using PdfManagement.API.Models.Documents;
using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Core.Domain.Entities;
using PdfManagement.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PdfManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Endpoints for managing PDF documents")]
    public class PdfDocumentsController : ControllerBase
    {
        private readonly IPdfDocumentService _pdfDocumentService;
        private readonly IGoogleStorageService _googleStorageService;
        private readonly ILogger<PdfDocumentsController> _logger;

        public PdfDocumentsController(
            IPdfDocumentService pdfDocumentService,
            IGoogleStorageService googleStorageService,
            ILogger<PdfDocumentsController> logger)
        {
            _pdfDocumentService = pdfDocumentService;
            _googleStorageService = googleStorageService;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "List of user documents", typeof(IEnumerable<DocumentViewModel>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<ActionResult<IEnumerable<DocumentViewModel>>> GetUserDocuments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            _logger.LogInformation($"Getting documents for user: {userId}");
            
            var documents = await _pdfDocumentService.GetUserPdfsAsync(userId);

            var result = new List<DocumentViewModel>();
            foreach (var doc in documents)
            {
                result.Add(new DocumentViewModel
                {
                    Id = doc.Id,
                    FileName = doc.FileName,
                    FileSize = doc.FileSize,
                    UploadedAt = doc.UploadedAt,
                    DownloadUrl = Url.ActionLink("DownloadDocument", "PdfDocuments", new { id = doc.Id }) ?? string.Empty,
                    ViewUrl = Url.ActionLink("ViewDocument", "PdfDocuments", new { id = doc.Id }) ?? string.Empty
                });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "Document details", typeof(DocumentViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<ActionResult<DocumentViewModel>> GetDocument(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            _logger.LogInformation($"Getting document {id} for user: {userId}");
            
            var document = await _pdfDocumentService.GetPdfByIdAsync(id);

            if (document == null || document.UploaderId != userId)
            {
                _logger.LogWarning($"Document {id} not found or not owned by user {userId}");
                return NotFound();
            }

            var result = new DocumentViewModel
            {
                Id = document.Id,
                FileName = document.FileName,
                FileSize = document.FileSize,
                UploadedAt = document.UploadedAt,
                DownloadUrl = Url.ActionLink("DownloadDocument", "PdfDocuments", new { id = document.Id }) ?? string.Empty,
                ViewUrl = Url.ActionLink("ViewDocument", "PdfDocuments", new { id = document.Id }) ?? string.Empty
            };

            return Ok(result);
        }

        [HttpPost]
        [SwaggerResponse(StatusCodes.Status201Created, "Document uploaded successfully", typeof(DocumentViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid file or file type")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error during upload")]
        public async Task<ActionResult<DocumentViewModel>> UploadDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Upload attempt with null or empty file");
                return BadRequest(new ApiResponse { Success = false, Message = "No file uploaded." });
            }

            if (file.ContentType != "application/pdf")
            {
                _logger.LogWarning($"Upload attempt with invalid file type: {file.ContentType}");
                return BadRequest(new ApiResponse { Success = false, Message = "Only PDF files are allowed." });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
                _logger.LogInformation($"Uploading document for user: {userId}, filename: {file.FileName}, size: {file.Length} bytes");
                
                var document = await _pdfDocumentService.UploadPdfAsync(file, userId);

                var result = new DocumentViewModel
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    FileSize = document.FileSize,
                    UploadedAt = document.UploadedAt,
                    DownloadUrl = Url.ActionLink("DownloadDocument", "PdfDocuments", new { id = document.Id }) ?? string.Empty
                };

                _logger.LogInformation($"Document uploaded successfully with ID: {document.Id}");
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Document deleted successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            _logger.LogInformation($"Deleting document {id} for user: {userId}");
            
            var result = await _pdfDocumentService.DeletePdfAsync(id, userId);

            if (!result)
            {
                _logger.LogWarning($"Document {id} not found or not owned by user {userId}");
                return NotFound(new ApiResponse { Success = false, Message = "Document not found or you don't have permission to delete it" });
            }

            _logger.LogInformation($"Document {id} deleted successfully");
            return NoContent();
        }

        [HttpPost("{id}/share")]
        [SwaggerResponse(StatusCodes.Status200OK, "Share link generated", typeof(ShareDocumentResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error")]
        public async Task<ActionResult<ShareDocumentResponse>> ShareDocument(int id, [FromBody] ShareDocumentModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            _logger.LogInformation($"Sharing document {id} for user: {userId}");

            try
            {
                // First check if the document exists and belongs to the user
                var document = await _pdfDocumentService.GetPdfByIdAsync(id);
                if (document == null || document.UploaderId != userId)
                {
                    _logger.LogWarning($"Document {id} not found or not owned by user {userId}");
                    return NotFound(new ApiResponse { Success = false, Message = "Document not found" });
                }

                // If model.ExpiresAt is null, set a default value
                if (model.ExpiresAt == null)
                {
                    model.ExpiresAt = DateTime.UtcNow.AddDays(7);
                }

                var token = await _pdfDocumentService.GenerateAccessTokenAsync(
                    id,
                    userId,
                    model.ExpiresAt);

                // Check if we're in production environment
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                string shareUrl;

                if (environment == "Production")
                {
                    // In production, use the FRONTEND_URL environment variable
                    var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
                    if (string.IsNullOrEmpty(frontendUrl))
                    {
                        frontendUrl = "https://pdf-frontend-three-beta.vercel.app"; // Fallback to hardcoded URL
                        _logger.LogWarning("FRONTEND_URL not set in production, using hardcoded fallback: {frontendUrl}");
                    }
                    
                    // Ensure no trailing slash
                    frontendUrl = frontendUrl.TrimEnd('/');
                    shareUrl = $"{frontendUrl}/shared-pdf/{token.Token}";
                    _logger.LogInformation($"Production share URL generated: {shareUrl}");
                }
                else
                {
                    // In development, use the local URL construction
                    var request = HttpContext.Request;
                    var host = request.Host.Value.Replace("5000", "3000");
                    var baseUrl = $"{request.Scheme}://{host}";
                    shareUrl = $"{baseUrl}/shared-pdf/{token.Token}";
                    _logger.LogInformation($"Development share URL generated: {shareUrl}");
                }

                var response = new ShareDocumentResponse
                {
                    Token = token.Token,
                    ExpiresAt = token.ExpiresAt,
                    Url = shareUrl
                };

                _logger.LogInformation($"Share link generated for document {id} with token {token.Token}");
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Bad request when sharing document {id}");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sharing document {id}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Success = false, Message = $"An error occurred while sharing the document: {ex.Message}" });
            }
        }

        [HttpPost("{id}/share-jwt")]
        [SwaggerResponse(StatusCodes.Status200OK, "JWT token generated", typeof(JwtShareResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error")]
        public async Task<ActionResult<JwtShareResponse>> ShareDocumentJwt(int id, [FromBody] ShareDocumentModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            _logger.LogInformation($"Generating JWT share token for document {id} for user: {userId}");

            try
            {
                // First check if the document exists and belongs to the user
                var document = await _pdfDocumentService.GetPdfByIdAsync(id);
                if (document == null || document.UploaderId != userId)
                {
                    _logger.LogWarning($"Document {id} not found or not owned by user {userId}");
                    return NotFound(new ApiResponse { Success = false, Message = "Document not found" });
                }

                // Generate a JWT token for the document
                var token = await _pdfDocumentService.GenerateAccessTokenAsync(id, userId, model.ExpiresAt);

                // Create a JWT token with document information
                var claims = new List<Claim>
                {
                    new Claim("documentId", id.ToString()),
                    new Claim("tokenId", token.Token.ToString()),
                    new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(token.ExpiresAt).ToUnixTimeSeconds().ToString())
                };

                // Get JWT key from environment variable
                var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
                if (string.IsNullOrEmpty(jwtKey))
                {
                    // Fallback to a default key (not recommended for production)
                    jwtKey = "ThisIsMySecretKeyForPdfManagementApplication12345ThisIsALongerKeyToMeetRequirements";
                    _logger.LogWarning("Using default JWT key. Set JWT_KEY environment variable for security.");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var jwtToken = new JwtSecurityToken(
                    issuer: Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "PdfManagement.API",
                    audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "PdfManagementClient",
                    claims: claims,
                    expires: token.ExpiresAt,
                    signingCredentials: creds);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

                // Check if we're in production environment
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                string shareUrl;

                if (environment == "Production")
                {
                    // In production, use the FRONTEND_URL environment variable
                    var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
                    if (string.IsNullOrEmpty(frontendUrl))
                    {
                        frontendUrl = "https://pdf-frontend-three-beta.vercel.app"; // Fallback to hardcoded URL
                        _logger.LogWarning("FRONTEND_URL not set in production, using hardcoded fallback: {frontendUrl}");
                    }
                    
                    // Ensure no trailing slash
                    frontendUrl = frontendUrl.TrimEnd('/');
                    shareUrl = $"{frontendUrl}/shared-pdf/{tokenString}";
                    _logger.LogInformation($"Production JWT share URL generated: {shareUrl}");
                }
                else
                {
                    // In development, use the local URL construction
                    var request = HttpContext.Request;
                    var host = request.Host.Value.Replace("5000", "3000");
                    var baseUrl = $"{request.Scheme}://{host}";
                    shareUrl = $"{baseUrl}/shared-pdf/{tokenString}";
                    _logger.LogInformation($"Development JWT share URL generated: {shareUrl}");
                }

                var response = new JwtShareResponse
                {
                    Token = tokenString,
                    ExpiresAt = token.ExpiresAt,
                    ShareUrl = shareUrl
                };

                _logger.LogInformation($"JWT share token generated for document {id}");
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Bad request when generating JWT share token for document {id}");
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating JWT share token for document {id}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Success = false, Message = $"An error occurred while sharing the document: {ex.Message}" });
            }
        }

        [HttpGet("download/{id}")]
        [SwaggerResponse(StatusCodes.Status200OK, "File content", typeof(FileContentResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                _logger.LogInformation($"Downloading document {id}");
                
                // For demo purposes, don't check user ID
                var document = await _pdfDocumentService.GetPdfByIdAsync(id);

                if (document == null)
                {
                    _logger.LogWarning($"Document {id} not found");
                    return NotFound(new ApiResponse { Success = false, Message = "Document not found" });
                }

                // Get file from Google Cloud Storage
                var fileBytes = await _googleStorageService.GetFileAsync(document.FilePath);

                // Set CORS headers to allow download from any origin
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Access-Control-Allow-Methods", "GET");
                Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                _logger.LogInformation($"Document {id} downloaded successfully, size: {fileBytes.Length} bytes");
                return File(fileBytes, document.ContentType, document.FileName);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, $"File not found for document {id}");
                return NotFound(new ApiResponse { Success = false, Message = "File not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading document {id}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Success = false, Message = $"An error occurred while downloading the file: {ex.Message}" });
            }
        }
        
        [HttpGet("view/{id}")]
        //[Authorize]
        [SwaggerOperation(
            Summary = "View a document",
            Description = "Returns a PDF document for viewing in the browser",
            OperationId = "ViewDocument",
            Tags = new[] { "Documents" }
              )]
        [SwaggerResponse(StatusCodes.Status200OK, "File content", typeof(FileContentResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        public async Task<IActionResult> ViewDocument(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            _logger.LogInformation($"Viewing document {id} for user: {userId}");
            
            var document = await _pdfDocumentService.GetPdfByIdAsync(id);

            if (document == null || document.UploaderId != userId)
            {
                _logger.LogWarning($"Document {id} not found or not owned by user {userId}");
                return NotFound(new ApiResponse { Success = false, Message = "Document not found" });
            }

            try
            {
                // Get file from Google Cloud Storage
                var fileBytes = await _googleStorageService.GetFileAsync(document.FilePath);

                _logger.LogInformation($"Document {id} viewed successfully, size: {fileBytes.Length} bytes");
                return File(fileBytes, "application/pdf", document.FileName, false);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, $"File not found for document {id}");
                return NotFound(new ApiResponse { Success = false, Message = "File not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error viewing document {id}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
