using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PdfManagement.API.Attributes;
using PdfManagement.API.Models.Comments;
using PdfManagement.API.Models.Common;
using PdfManagement.API.Models.Public;
using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    [SwaggerTag("Public endpoints for accessing shared documents")]
    public class PublicController : ControllerBase
    {
        private readonly IPublicAccessService _publicAccessService;
        private readonly IGoogleStorageService _googleStorageService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PublicController> _logger;

        public PublicController(
            IPublicAccessService publicAccessService,
            IGoogleStorageService googleStorageService,
            IConfiguration configuration,
            ILogger<PublicController> logger)
        {
            _publicAccessService = publicAccessService;
            _googleStorageService = googleStorageService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("view/{token}")]
        [SwaggerOperation(
            Summary = "View a shared document",
            Description = "Retrieves a document using a share token",
            OperationId = "ViewSharedDocument",
            Tags = new[] { "Public" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Document details and comments", typeof(PublicDocumentViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found or token expired")]
        [ResponseCache(Duration = 60)] // Cache for 1 minute
        public async Task<IActionResult> View(Guid token)
        {
            _logger.LogInformation($"View request received for token: {token}");
            
            var document = await _publicAccessService.GetDocumentByTokenAsync(
                token,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString());

            if (document == null)
            {
                _logger.LogWarning($"Document not found for token: {token}");
                return NotFound(new ApiResponse { Success = false, Message = "Document not found or access token has expired" });
            }

            // Get comments for the document
            var comments = await _publicAccessService.GetCommentsForTokenAsync(token);
            var commentViewModels = comments?.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                PageNumber = c.PageNumber,
                CreatedAt = c.CreatedAt,
                UserType = c.UserType,
                CommenterId = c.CommenterId,
                CommenterName = c.Commenter?.UserName,
                ParentCommentId = c.ParentCommentId,
                // Map replies with full details to ensure they're properly displayed
                Replies = c.Replies?.Select(r => new CommentViewModel
                {
                    Id = r.Id,
                    Content = r.Content,
                    PageNumber = r.PageNumber,
                    CreatedAt = r.CreatedAt,
                    UserType = r.UserType,
                    CommenterId = r.CommenterId,
                    CommenterName = r.Commenter?.UserName,
                    ParentCommentId = r.ParentCommentId,
                    // Include nested replies if any (though typically replies don't have their own replies)
                    Replies = r.Replies?.Select(nr => new CommentViewModel
                    {
                        Id = nr.Id,
                        Content = nr.Content,
                        PageNumber = nr.PageNumber,
                        CreatedAt = nr.CreatedAt,
                        UserType = nr.UserType,
                        CommenterId = nr.CommenterId,
                        CommenterName = nr.Commenter?.UserName,
                        ParentCommentId = nr.ParentCommentId
                    }).ToList() ?? new System.Collections.Generic.List<CommentViewModel>()
                }).ToList() ?? new System.Collections.Generic.List<CommentViewModel>()
            }).ToList() ?? new System.Collections.Generic.List<CommentViewModel>();

            var response = new PublicDocumentViewModel
            {
                Document = new DocumentInfo
                {
                    Id = document.Id,
                    FileName = document.FileName,
                    UploadedAt = document.UploadedAt
                },
                Comments = commentViewModels,
                DownloadUrl = Url.ActionLink("Download", "Public", new { token = token }) ?? string.Empty
            };

            _logger.LogInformation($"Returning document info: {document.FileName}, ID: {document.Id}");
            return Ok(response);
        }

        [HttpGet("view-jwt/{token}")]
        [SwaggerOperation(
            Summary = "View a shared document using JWT",
            Description = "Retrieves a document using a JWT token",
            OperationId = "ViewSharedDocumentJwt",
            Tags = new[] { "Public" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "File content", typeof(FileContentResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found or token expired")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid token")]
        public async Task<IActionResult> ViewWithJwt(string token)
        {
            _logger.LogInformation($"JWT view request received for token: {token.Substring(0, Math.Min(token.Length, 20))}...");
            
            try
            {
                // Validate and decode the JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("JWT_KEY") ?? "your_default_jwt_key_for_pdf_sharing_12345");

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "pdf-management-api",
                    ValidateAudience = true,
                    ValidAudience = "pdf-management-client",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                // Extract document ID from claims
                var documentIdClaim = principal.FindFirst("documentId");
                if (documentIdClaim == null || !int.TryParse(documentIdClaim.Value, out int documentId))
                {
                    _logger.LogWarning("Invalid token: missing or invalid document ID");
                    return BadRequest(new ApiResponse { Success = false, Message = "Invalid token: missing or invalid document ID" });
                }

                // Extract token ID from claims
                var tokenIdClaim = principal.FindFirst("tokenId");
                if (tokenIdClaim == null || !Guid.TryParse(tokenIdClaim.Value, out Guid tokenId))
                {
                    _logger.LogWarning("Invalid token: missing or invalid token ID");
                    return BadRequest(new ApiResponse { Success = false, Message = "Invalid token: missing or invalid token ID" });
                }

                // Get the document using the token ID
                var document = await _publicAccessService.GetDocumentByTokenAsync(
                    tokenId,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString());

                if (document == null || document.Id != documentId)
                {
                    _logger.LogWarning($"Document not found for token ID: {tokenId}, document ID: {documentId}");
                    return NotFound(new ApiResponse { Success = false, Message = "Document not found or access token has expired" });
                }

                // Return the PDF file for viewing
                try
                {
                    _logger.LogInformation($"Fetching file from path: {document.FilePath}");
                    var fileBytes = await _googleStorageService.GetFileAsync(document.FilePath);
                    
                    // Set CORS headers to allow viewing from any origin
                    Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    Response.Headers.Add("Access-Control-Allow-Methods", "GET");
                    Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                    
                    _logger.LogInformation($"Returning file: {document.FileName}, Content-Type: {document.ContentType}, Size: {fileBytes.Length} bytes");
                    return File(fileBytes, "application/pdf", document.FileName, false);
                }
                catch (FileNotFoundException ex)
                {
                    _logger.LogWarning(ex, $"File not found: {ex.Message}");
                    return NotFound(new ApiResponse { Success = false, Message = "File not found" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error retrieving file: {ex.Message}");
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" });
                }
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, $"Token expired: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Token has expired" });
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, $"Invalid token: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = $"Invalid token: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing JWT: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("download/{token}")]
        [SwaggerOperation(
            Summary = "Download a shared document",
            Description = "Downloads a document using a share token",
            OperationId = "DownloadSharedDocument",
            Tags = new[] { "Public" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "File content", typeof(FileContentResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found or token expired")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error")]
        [ResponseCache(Duration = 300)] // Cache for 5 minutes
        public async Task<IActionResult> Download(Guid token)
        {
            _logger.LogInformation($"Download request received for token: {token}");
            
            var document = await _publicAccessService.GetDocumentByTokenAsync(
                token,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString());

            if (document == null)
            {
                _logger.LogWarning($"Document not found for token: {token}");
                return NotFound(new ApiResponse { Success = false, Message = "Document not found or access token has expired" });
            }

            try
            {
                _logger.LogInformation($"Fetching file from path: {document.FilePath}");
                var fileBytes = await _googleStorageService.GetFileAsync(document.FilePath);
                
                // Set CORS headers to allow download from any origin
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
                Response.Headers.Add("Access-Control-Allow-Methods", "GET");
                Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
                
                _logger.LogInformation($"Returning file: {document.FileName}, Content-Type: {document.ContentType}, Size: {fileBytes.Length} bytes");
                return File(fileBytes, document.ContentType, document.FileName);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, $"File not found: {ex.Message}");
                return NotFound(new ApiResponse { Success = false, Message = "File not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Success = false, Message = "An error occurred while downloading the file" });
            }
        }

        [HttpGet("shared/{token}")]
        [SwaggerOperation(
            Summary = "View a shared document in HTML viewer",
            Description = "Displays a shared document in the HTML viewer",
            OperationId = "ViewSharedDocumentHtml",
            Tags = new[] { "Public" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "HTML page for viewing the document")]
        public IActionResult ViewSharedHtml(Guid token)
        {
            return File("~/shared-document.html", "text/html");
        }

        [HttpPost("comment/{token}")]
        [SwaggerOperation(
            Summary = "Add a comment to a shared document",
            Description = "Adds a comment to a document using a share token",
            OperationId = "AddCommentToSharedDocument",
            Tags = new[] { "Public" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Comment added successfully", typeof(CommentViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found or token expired")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error")]
        [RateLimit(Name = "PublicComment", Seconds = 60, Limit = 5)]
        public async Task<IActionResult> AddComment(Guid token, [FromBody] AddCommentRequest model)
        {
            _logger.LogInformation($"Add comment request received for token: {token}");
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comment = await _publicAccessService.AddCommentToTokenDocumentAsync(
                token,
                model.Content,
                model.PageNumber,
                model.ParentCommentId,
                model.CommenterName);

            if (comment == null)
            {
                _logger.LogWarning($"Document not found for token: {token}");
                return NotFound(new ApiResponse { Success = false, Message = "Document not found or access token has expired" });
            }

            // After adding the comment, get all updated comments for the document
            var allComments = await _publicAccessService.GetCommentsForTokenAsync(token);
            var updatedCommentViewModels = allComments?.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Content = c.Content,
                PageNumber = c.PageNumber,
                CreatedAt = c.CreatedAt,
                UserType = c.UserType,
                CommenterId = c.CommenterId,
                CommenterName = c.Commenter?.UserName,
                ParentCommentId = c.ParentCommentId,
                Replies = c.Replies?.Select(r => new CommentViewModel
                {
                    Id = r.Id,
                    Content = r.Content,
                    PageNumber = r.PageNumber,
                    CreatedAt = r.CreatedAt,
                    UserType = r.UserType,
                    CommenterId = r.CommenterId,
                    CommenterName = r.Commenter?.UserName,
                    ParentCommentId = r.ParentCommentId
                }).ToList() ?? new System.Collections.Generic.List<CommentViewModel>()
            }).ToList() ?? new System.Collections.Generic.List<CommentViewModel>();

            // Create a view model for the newly added comment
            var newCommentViewModel = new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                PageNumber = comment.PageNumber,
                CreatedAt = comment.CreatedAt,
                UserType = comment.UserType,
                ParentCommentId = comment.ParentCommentId,
                Replies = new System.Collections.Generic.List<CommentViewModel>()
            };

            _logger.LogInformation($"Comment added successfully: ID {comment.Id}");
            return Ok(new
            {
                comment = newCommentViewModel,
                allComments = updatedCommentViewModels,
                success = true,
                message = comment.ParentCommentId.HasValue ? "Reply added successfully" : "Comment added successfully"
            });
        }

        [HttpPost("comment-jwt/{token}")]
        [SwaggerOperation(
            Summary = "Add a comment to a shared document using JWT",
            Description = "Adds a comment to a document using a JWT token",
            OperationId = "AddCommentToSharedDocumentJwt",
            Tags = new[] { "Public" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Comment added successfully", typeof(CommentViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found or token expired")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input")]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Server error")]
        public async Task<IActionResult> AddCommentJwt(string token, [FromBody] AddCommentRequest model)
        {
            _logger.LogInformation($"Add comment JWT request received for token: {token.Substring(0, Math.Min(token.Length, 20))}...");
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Validate and decode the JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(
                    Environment.GetEnvironmentVariable("JWT_KEY") ?? "your_default_jwt_key_for_pdf_sharing_12345");

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "pdf-management-api",
                    ValidateAudience = true,
                    ValidAudience = "pdf-management-client",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                // Extract token ID from claims
                var tokenIdClaim = principal.FindFirst("tokenId");
                if (tokenIdClaim == null || !Guid.TryParse(tokenIdClaim.Value, out Guid tokenId))
                {
                    _logger.LogWarning("Invalid token: missing or invalid token ID");
                    return BadRequest(new ApiResponse { Success = false, Message = "Invalid token: missing or invalid token ID" });
                }

                // Add comment using the token ID
                var comment = await _publicAccessService.AddCommentToTokenDocumentAsync(
                    tokenId,
                    model.Content,
                    model.PageNumber,
                    model.ParentCommentId,
                    model.CommenterName);

                if (comment == null)
                {
                    _logger.LogWarning($"Document not found for token ID: {tokenId}");
                    return NotFound(new ApiResponse { Success = false, Message = "Document not found or access token has expired" });
                }

                // After adding the comment, get all updated comments for the document
                var allComments = await _publicAccessService.GetCommentsForTokenAsync(tokenId);
                var updatedCommentViewModels = allComments?.Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    Content = c.Content,
                    PageNumber = c.PageNumber,
                    CreatedAt = c.CreatedAt,
                    UserType = c.UserType,
                    CommenterId = c.CommenterId,
                    CommenterName = c.Commenter?.UserName,
                    ParentCommentId = c.ParentCommentId,
                    Replies = c.Replies?.Select(r => new CommentViewModel
                    {
                        Id = r.Id,
                        Content = r.Content,
                        PageNumber = r.PageNumber,
                        CreatedAt = r.CreatedAt,
                        UserType = r.UserType,
                        CommenterId = r.CommenterId,
                        CommenterName = r.Commenter?.UserName,
                        ParentCommentId = r.ParentCommentId
                    }).ToList() ?? new System.Collections.Generic.List<CommentViewModel>()
                }).ToList() ?? new System.Collections.Generic.List<CommentViewModel>();

                // Create a view model for the newly added comment
                var newCommentViewModel = new CommentViewModel
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    PageNumber = comment.PageNumber,
                    CreatedAt = comment.CreatedAt,
                    UserType = comment.UserType,
                    ParentCommentId = comment.ParentCommentId,
                    Replies = new System.Collections.Generic.List<CommentViewModel>()
                };

                _logger.LogInformation($"Comment added successfully via JWT: ID {comment.Id}");
                return Ok(new
                {
                    comment = newCommentViewModel,
                    allComments = updatedCommentViewModels,
                    success = true,
                    message = comment.ParentCommentId.HasValue ? "Reply added successfully" : "Comment added successfully"
                });
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning(ex, $"Token expired: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = "Token has expired" });
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, $"Invalid token: {ex.Message}");
                return BadRequest(new ApiResponse { Success = false, Message = $"Invalid token: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding comment via JWT: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
