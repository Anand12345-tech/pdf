using Microsoft.EntityFrameworkCore;
using PdfManagement.Core.Domain.Entities;
using PdfManagement.Core.Domain.Interfaces;
using PdfManagement.Data.Repositories.Interfaces;
using PdfManagement.Infrastructure.Data.Context;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfManagement.Data.Repositories.Implementations
{
    /// <summary>
    /// Implementation of the PDF comment repository
    /// </summary>
    public class PdfCommentRepository : IPdfCommentRepository
    {
        private readonly ApplicationDbContext _context;

        public PdfCommentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<PdfComment> GetByIdAsync(int id)
        {
            return await _context.PdfComments
                .Include(c => c.Commenter)
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PdfComment>> GetByPdfIdAsync(int pdfId)
        {
            return await _context.PdfComments
                .Include(c => c.Commenter)
                .Include(c => c.Replies)
                .ThenInclude(r => r.Commenter)
                .Where(c => c.DocumentId == pdfId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PdfComment>> GetRepliesAsync(int commentId)
        {
            return await _context.PdfComments
                .Include(c => c.Commenter)
                .Where(c => c.ParentCommentId == commentId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<PdfComment> AddAsync(PdfComment comment)
        {
            _context.PdfComments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload the comment with the commenter
            if (comment.CommenterId != null)
            {
                return await _context.PdfComments
                    .Include(c => c.Commenter)
                    .FirstOrDefaultAsync(c => c.Id == comment.Id);
            }

            return comment;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(PdfComment comment)
        {
            _context.PdfComments.Update(comment);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id)
        {
            var comment = await _context.PdfComments.FindAsync(id);

            if (comment == null)
            {
                return false;
            }

            // Delete all replies first
            var replies = await _context.PdfComments
                .Where(c => c.ParentCommentId == id)
                .ToListAsync();

            if (replies.Any())
            {
                _context.PdfComments.RemoveRange(replies);
            }

            _context.PdfComments.Remove(comment);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}