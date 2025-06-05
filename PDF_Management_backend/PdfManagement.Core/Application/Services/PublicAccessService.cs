using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Core.Domain.Entities;
using PdfManagement.Core.Domain.Interfaces;
using PdfManagement.Data.Repositories.Interfaces;
using PdfManagement.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PdfManagement.Services.Implementations
{
    /// <summary>
    /// Implementation of the public access service
    /// </summary>
    public class PublicAccessService : IPublicAccessService
    {
        private readonly IPdfDocumentRepository _documentRepository;
        private readonly IPdfCommentRepository _commentRepository;
        private readonly IPdfCommentService _commentService;

        public PublicAccessService(
            IPdfDocumentRepository documentRepository,
            IPdfCommentRepository commentRepository,
            IPdfCommentService commentService)
        {
            _documentRepository = documentRepository;
            _commentRepository = commentRepository;
            _commentService = commentService;
        }

        /// <inheritdoc/>
        public async Task<PdfDocument> GetDocumentByTokenAsync(Guid token, string ipAddress = null, string userAgent = null)
        {
            var document = await _documentRepository.GetByTokenAsync(token);

            if (document != null)
            {
                // Log the access
                await _documentRepository.LogAccessAsync(new PdfAccessLog
                {
                    DocumentId = document.Id,
                    AccessedAt = DateTime.UtcNow,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                });
            }

            return document;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PdfComment>> GetCommentsForTokenAsync(Guid token)
        {
            var document = await _documentRepository.GetByTokenAsync(token);

            if (document == null)
            {
                return null;
            }

            return await _commentRepository.GetByPdfIdAsync(document.Id);
        }

        /// <inheritdoc/>
        public async Task<PdfComment> AddCommentToTokenDocumentAsync(
            Guid token,
            string content,
            int pageNumber,
            int? parentCommentId = null,
            string commenterName = null)
        {
            var document = await _documentRepository.GetByTokenAsync(token);

            if (document == null)
            {
                return null;
            }

            try
            {
                return await _commentService.AddCommentAsync(
                    document.Id,
                    content,
                    pageNumber,
                    null, // No user ID for public comments
                    "invited",
                    parentCommentId,
                    commenterName);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}