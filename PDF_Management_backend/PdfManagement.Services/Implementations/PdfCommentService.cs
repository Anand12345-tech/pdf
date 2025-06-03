using PdfManagement.Data.Models;
using PdfManagement.Data.Repositories.Interfaces;
using PdfManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Services.Implementations
{
    /// <summary>
    /// Implementation of the PDF comment service
    /// </summary>
    public class PdfCommentService : IPdfCommentService
    {
        private readonly IPdfCommentRepository _commentRepository;
        private readonly IPdfDocumentRepository _documentRepository;

        public PdfCommentService(
            IPdfCommentRepository commentRepository,
            IPdfDocumentRepository documentRepository)
        {
            _commentRepository = commentRepository;
            _documentRepository = documentRepository;
        }

        /// <inheritdoc/>
        public async Task<PdfComment> AddCommentAsync(
            int pdfId, 
            string content, 
            int pageNumber, 
            string userId = null, 
            string userType = "user", 
            int? parentCommentId = null,
            string commenterName = null)
        {
            // Validate document exists
            var document = await _documentRepository.GetByIdAsync(pdfId);
            if (document == null)
            {
                throw new ArgumentException("Document not found.");
            }

            // Validate parent comment if provided
            if (parentCommentId.HasValue)
            {
                var parentComment = await _commentRepository.GetByIdAsync(parentCommentId.Value);
                if (parentComment == null)
                {
                    throw new ArgumentException("Parent comment not found.");
                }
                
                // Ensure parent comment belongs to the same document
                if (parentComment.DocumentId != pdfId)
                {
                    throw new ArgumentException("Parent comment does not belong to the specified document.");
                }
                
                // Prevent nested replies (only one level of nesting allowed)
                if (parentComment.ParentCommentId.HasValue)
                {
                    throw new ArgumentException("Nested replies are not allowed. You can only reply to top-level comments.");
                }
            }

            var comment = new PdfComment
            {
                DocumentId = pdfId,
                Content = content,
                PageNumber = pageNumber,
                CommenterId = userId,
                UserType = userType,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = parentCommentId,
                CommenterName = commenterName
            };

            return await _commentRepository.AddAsync(comment);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PdfComment>> GetCommentsForPdfAsync(int pdfId)
        {
            return await _commentRepository.GetByPdfIdAsync(pdfId);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PdfComment>> GetCommentRepliesAsync(int commentId)
        {
            return await _commentRepository.GetRepliesAsync(commentId);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            
            if (comment == null)
            {
                return false;
            }
            
            // Check if user has permission to delete
            if (comment.CommenterId != userId)
            {
                // Check if user is the document owner
                var document = await _documentRepository.GetByIdAsync(comment.DocumentId);
                if (document == null || document.UploaderId != userId)
                {
                    return false;
                }
            }
            
            return await _commentRepository.DeleteAsync(commentId);
        }

        /// <inheritdoc/>
        public async Task<PdfComment> UpdateCommentAsync(int commentId, string content, string userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            
            if (comment == null || comment.CommenterId != userId)
            {
                return null;
            }
            
            comment.Content = content;
            comment.UpdatedAt = DateTime.UtcNow;
            
            var success = await _commentRepository.UpdateAsync(comment);
            return success ? comment : null;
        }
    }
}
