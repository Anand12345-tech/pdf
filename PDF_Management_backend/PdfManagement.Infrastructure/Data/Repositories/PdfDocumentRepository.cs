using Microsoft.EntityFrameworkCore;
using PdfManagement.Core.Domain.Entities;
using PdfManagement.Core.Domain.Interfaces;
using PdfManagement.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfManagement.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Implementation of the PDF document repository
    /// </summary>
    public class PdfDocumentRepository : Repository<PdfDocument>, IPdfDocumentRepository
    {
        public PdfDocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public override async Task<PdfDocument?> GetByIdAsync(object id)
        {
            return await _context.PdfDocuments
                .Include(d => d.Uploader)
                .FirstOrDefaultAsync(d => d.Id == (int)id);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<PdfDocument>> GetByUserIdAsync(string userId)
        {
            return await _context.PdfDocuments
                .Where(d => d.UploaderId == userId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteWithUserVerificationAsync(int id, string userId)
        {
            var document = await _context.PdfDocuments
                .FirstOrDefaultAsync(d => d.Id == id && d.UploaderId == userId);

            if (document == null)
            {
                return false;
            }

            _context.PdfDocuments.Remove(document);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc/>
        public async Task<PdfAccessToken> CreateAccessTokenAsync(PdfAccessToken token)
        {
            _context.PdfAccessTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        /// <inheritdoc/>
        public async Task<PdfDocument?> GetByTokenAsync(Guid token)
        {
            var accessToken = await _context.PdfAccessTokens
                .Include(t => t.Document)
                .ThenInclude(d => d!.Uploader)
                .FirstOrDefaultAsync(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow && !t.IsRevoked);

            return accessToken?.Document;
        }

        /// <inheritdoc/>
        public async Task<PdfAccessToken?> GetAccessTokenAsync(Guid token)
        {
            return await _context.PdfAccessTokens
                .Include(t => t.Document)
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAccessTokenAsync(PdfAccessToken token)
        {
            _context.PdfAccessTokens.Update(token);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc/>
        public async Task<PdfAccessLog> LogAccessAsync(PdfAccessLog log)
        {
            _context.PdfAccessLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }
    }
}
