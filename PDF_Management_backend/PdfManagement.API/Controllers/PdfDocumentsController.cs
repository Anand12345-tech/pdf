using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PdfManagement.API.Models.Common;
using PdfManagement.API.Models.Documents;
using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Core.Domain.Entities;
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
        private readonly IFileStorageService _fileStorageService;

        public PdfDocumentsController(
            IPdfDocumentService pdfDocumentService,
            IFileStorageService fileStorageService)
        {
            _pdfDocumentService = pdfDocumentService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
       // [Authorize]
        [SwaggerOperation(
            Summary = "Get all documents for the current user",
            Description = "Returns a list of all PDF documents uploaded by the authenticated user",
            OperationId = "GetUserDocuments",
            Tags = new[] { "Documents" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "List of user documents", typeof(IEnumerable<DocumentViewModel>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<ActionResult<IEnumerable<DocumentViewModel>>> GetUserDocuments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
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
        //[Authorize]
        [SwaggerOperation(
            Summary = "Get a specific document",
            Description = "Returns details of a specific PDF document by ID",
            OperationId = "GetDocument",
            Tags = new[] { "Documents" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Document details", typeof(DocumentViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<ActionResult<DocumentViewModel>> GetDocument(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            var document = await _pdfDocumentService.GetPdfByIdAsync(id);

            if (document == null || document.UploaderId != userId)
            {
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
        //[Authorize]
        [SwaggerOperation(
            Summary = "Upload a new PDF document",
            Description = "Uploads a new PDF document to the system",
            OperationId = "UploadDocument",
            Tags = new[] { "Documents" }
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Document uploaded successfully", typeof(DocumentViewModel))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid file or file type")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error during upload")]
        public async Task<ActionResult<DocumentViewModel>> UploadDocument(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new ApiResponse { Success = false, Message = "No file uploaded." });
            }

            if (file.ContentType != "application/pdf")
            {
                return BadRequest(new ApiResponse { Success = false, Message = "Only PDF files are allowed." });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
                var document = await _pdfDocumentService.UploadPdfAsync(file, userId);
                
                var result = new DocumentViewModel
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    FileSize = document.FileSize,
                    UploadedAt = document.UploadedAt,
                    DownloadUrl = Url.ActionLink("DownloadDocument", "PdfDocuments", new { id = document.Id }) ?? string.Empty
                };
                
                return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        //[Authorize]
        [SwaggerOperation(
            Summary = "Delete a document",
            Description = "Deletes a PDF document by ID",
            OperationId = "DeleteDocument",
            Tags = new[] { "Documents" }
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Document deleted successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            var result = await _pdfDocumentService.DeletePdfAsync(id, userId);

            if (!result)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Document not found or you don't have permission to delete it" });
            }

            return NoContent();
        }

        [HttpPost("{id}/share")]
        //[Authorize]
        [SwaggerOperation(
            Summary = "Share a document",
            Description = "Generates a shareable link for a PDF document",
            OperationId = "ShareDocument",
            Tags = new[] { "Documents" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Share link generated", typeof(ShareDocumentResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error")]
        public async Task<ActionResult<ShareDocumentResponse>> ShareDocument(int id, [FromBody] ShareDocumentModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            
            try
            {
                // First check if the document exists and belongs to the user
                var document = await _pdfDocumentService.GetPdfByIdAsync(id);
                if (document == null || document.UploaderId != userId)
                {
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
                
                // Generate a frontend URL instead of an API endpoint URL
                var request = HttpContext.Request;
                // Replace API port (5000) with frontend port (3000)
                var host = request.Host.Value.Replace("5000", "3000");
                var baseUrl = $"{request.Scheme}://{host}";
                
                var response = new ShareDocumentResponse
                {
                    Token = token.Token,
                    ExpiresAt = token.ExpiresAt,
                    Url = $"{baseUrl}/shared-pdf/{token.Token}"
                };
                
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiResponse { Success = false, Message = $"An error occurred while sharing the document: {ex.Message}" });
            }
        }
        
        [HttpPost("{id}/share-jwt")]
        //[Authorize]
        [SwaggerOperation(
            Summary = "Share a document with JWT",
            Description = "Generates a JWT token for sharing a PDF document",
            OperationId = "ShareDocumentJwt",
            Tags = new[] { "Documents" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "JWT token generated", typeof(JwtShareResponse))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error")]
        public async Task<ActionResult<JwtShareResponse>> ShareDocumentJwt(int id, [FromBody] ShareDocumentModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            
            try
            {
                // First check if the document exists and belongs to the user
                var document = await _pdfDocumentService.GetPdfByIdAsync(id);
                if (document == null || document.UploaderId != userId)
                {
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
                    Console.WriteLine("WARNING: Using default JWT key. Set JWT_KEY environment variable for security.");
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
                
                // Create the response with the JWT token and share URL
                // Ensure we create an absolute URL that will work when pasted in any browser
                var request = HttpContext.Request;
                // Replace API port (5000) with frontend port (3000)
                var host = request.Host.Value.Replace("5000", "3000");
                var baseUrl = $"{request.Scheme}://{host}";
                var shareUrl = $"{baseUrl}/shared-pdf/{tokenString}";
                
                var response = new JwtShareResponse
                {
                    Token = tokenString,
                    ExpiresAt = token.ExpiresAt,
                    ShareUrl = shareUrl
                };
                
                return Ok(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new ApiResponse { Success = false, Message = $"An error occurred while sharing the document: {ex.Message}" });
            }
        }

        [HttpGet("download/{id}")]
        //[Authorize]
        [SwaggerOperation(
            Summary = "Download a document",
            Description = "Downloads a PDF document by ID",
            OperationId = "DownloadDocument",
            Tags = new[] { "Documents" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "File content", typeof(FileContentResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error")]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            try
            {
                // For demo purposes, don't check user ID
                var document = await _pdfDocumentService.GetPdfByIdAsync(id);

                if (document == null)
                {
                    return NotFound(new ApiResponse { Success = false, Message = "Document not found" });
                }

                var fileBytes = await _fileStorageService.GetFileAsync(document.FilePath);
                
                // Set CORS headers to allow download from any origin
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Access-Control-Allow-Methods", "GET");
                Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                
                return File(fileBytes, document.ContentType, document.FileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new ApiResponse { Success = false, Message = "File not found" });
            }
            catch (Exception ex)
            {
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
            var document = await _pdfDocumentService.GetPdfByIdAsync(id);

            if (document == null || document.UploaderId != userId)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Document not found" });
            }

            try
            {
                var fileBytes = await _fileStorageService.GetFileAsync(document.FilePath);
             
                return File(fileBytes, "application/pdf", document.FileName, false);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new ApiResponse { Success = false, Message = "File not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
