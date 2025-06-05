using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PdfManagement.API.Models.Comments;
using PdfManagement.API.Models.Common;
using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Core.Domain.Entities;
using PdfManagement.Services.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PdfManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Endpoints for managing document comments")]
    public class CommentsController : ControllerBase
    {
        private readonly IPdfCommentService _commentService;
        private readonly IPdfDocumentService _documentService;

        public CommentsController(
            IPdfCommentService commentService,
            IPdfDocumentService documentService)
        {
            _commentService = commentService;
            _documentService = documentService;
        }

        [HttpGet("document/{documentId}")]
        //[SwaggerOperation(
        //    Summary = "Get comments for a document",
        //    Description = "Returns all comments for a specific document",
        //    OperationId = "GetDocumentComments",
        //    Tags = new[] { "Comments" }
        //)]
        [SwaggerResponse(StatusCodes.Status200OK, "List of comments", typeof(IEnumerable<CommentViewModel>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<ActionResult<IEnumerable<CommentViewModel>>> GetDocumentComments(int documentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            var document = await _documentService.GetPdfByIdAsync(documentId);

            if (document == null || document.UploaderId != userId)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Document not found" });
            }

            var comments = await _commentService.GetCommentsForPdfAsync(documentId);
            var result = comments.Select(MapToCommentViewModel).ToList();

            return Ok(result);
        }

        [HttpGet("replies/{commentId}")]
        //[SwaggerOperation(
        //    Summary = "Get replies to a comment",
        //    Description = "Returns all replies to a specific comment",
        //    OperationId = "GetCommentReplies",
        //    Tags = new[] { "Comments" }
        //)]
        [SwaggerResponse(StatusCodes.Status200OK, "List of replies", typeof(IEnumerable<CommentViewModel>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<ActionResult<IEnumerable<CommentViewModel>>> GetCommentReplies(int commentId)
        {
            var replies = await _commentService.GetCommentRepliesAsync(commentId);
            var result = replies.Select(MapToCommentViewModel).ToList();

            return Ok(result);
        }

        [HttpPost("document/{documentId}")]
        //[SwaggerOperation(
        //    Summary = "Add a comment to a document",
        //    Description = "Adds a new comment to a specific document",
        //    OperationId = "AddComment",
        //    Tags = new[] { "Comments" }
        //)]
        [SwaggerResponse(StatusCodes.Status201Created, "Comment added successfully", typeof(CommentViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Document not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<ActionResult<CommentViewModel>> AddComment(int documentId, [FromBody] AddCommentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            var document = await _documentService.GetPdfByIdAsync(documentId);

            if (document == null || document.UploaderId != userId)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Document not found" });
            }

            try
            {
                var comment = await _commentService.AddCommentAsync(
                    documentId,
                    model.Content,
                    model.PageNumber,
                    userId,
                    "user",
                    model.ParentCommentId);

                var result = MapToCommentViewModel(comment);

                return CreatedAtAction(nameof(GetCommentReplies), new { commentId = comment.Id }, result);
            }
            catch (System.ArgumentException ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        [HttpPut("{commentId}")]
        [SwaggerOperation(
            Summary = "Update a comment",
            Description = "Updates the content of a specific comment",
            OperationId = "UpdateComment",
            Tags = new[] { "Comments" }
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Comment updated successfully", typeof(CommentViewModel))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Comment not found")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid input")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] UpdateCommentRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            var updatedComment = await _commentService.UpdateCommentAsync(commentId, model.Content, userId);

            if (updatedComment == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Comment not found or you don't have permission to update it" });
            }

            var result = MapToCommentViewModel(updatedComment);
            return Ok(result);
        }

        [HttpDelete("{commentId}")]
        [SwaggerOperation(
            Summary = "Delete a comment",
            Description = "Deletes a specific comment",
            OperationId = "DeleteComment",
            Tags = new[] { "Comments" }
        )]
        [SwaggerResponse(StatusCodes.Status204NoContent, "Comment deleted successfully")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Comment not found")]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User is not authenticated")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "demo-user";
            var result = await _commentService.DeleteCommentAsync(commentId, userId);

            if (!result)
            {
                return NotFound(new ApiResponse { Success = false, Message = "Comment not found or you don't have permission to delete it" });
            }

            return NoContent();
        }

        private CommentViewModel MapToCommentViewModel(PdfComment comment)
        {
            return new CommentViewModel
            {
                Id = comment.Id,
                Content = comment.Content,
                PageNumber = comment.PageNumber,
                CreatedAt = comment.CreatedAt,
                UserType = comment.UserType,
                CommenterId = comment.CommenterId,
                CommenterName = comment.Commenter?.UserName,
                ParentCommentId = comment.ParentCommentId,
                Replies = comment.Replies?.Select(MapToCommentViewModel).ToList() ?? new List<CommentViewModel>()
            };
        }
    }
}