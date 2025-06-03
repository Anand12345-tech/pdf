using Microsoft.EntityFrameworkCore;
using PdfManagement.Data.Models;
using PdfManagement.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PdfManagement.Data.Repositories.Implementations
{
    /// <summary>
    /// Implementation of the PDF document repository
    /// </summary>
    public class PdfDocumentRepository : IPdfDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public PdfDocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<PdfDocument> GetByIdAsync(int id)
        {
            return await _context.PdfDocuments
                .Include(d => d.Uploader)
                .FirstOrDefaultAsync(d => d.Id == id);
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
        public async Task<PdfDocument> AddAsync(PdfDocument document)
        {
            _context.PdfDocuments.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(PdfDocument document)
        {
            _context.PdfDocuments.Update(document);
            return await _context.SaveChangesAsync() > 0;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(int id, string userId)
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
        public async Task<PdfDocument> GetByTokenAsync(Guid token)
        {
            var accessToken = await _context.PdfAccessTokens
                .Include(t => t.Document)
                .ThenInclude(d => d.Uploader)
                .FirstOrDefaultAsync(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow && !t.IsRevoked);

            return accessToken?.Document;
        }

        /// <inheritdoc/>
        public async Task<PdfAccessToken> GetAccessTokenAsync(Guid token)
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
