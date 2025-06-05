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
            try
            {
                // Check if the document exists
                var document = await _context.PdfDocuments.FindAsync(token.DocumentId);
                if (document == null)
                {
                    throw new ArgumentException($"Document with ID {token.DocumentId} not found.");
                }

                // Ensure DateTime values have no Kind specification since we're using timestamp without time zone
                // This matches the database schema change you made
                token.CreatedAt = DateTime.SpecifyKind(token.CreatedAt, DateTimeKind.Unspecified);
                token.ExpiresAt = DateTime.SpecifyKind(token.ExpiresAt, DateTimeKind.Unspecified);

                // Add the token to the context
                _context.PdfAccessTokens.Add(token);
                
                try {
                    // Save changes and handle any exceptions
                    await _context.SaveChangesAsync();
                    return token;
                }
                catch (Exception ex)
                {
                    // Log more detailed exception information
                    Console.WriteLine($"Database save error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception details: {ex.InnerException.Message}");
                        Console.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                    }
                    throw; // Re-throw the exception to be handled by the caller
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error creating access token: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                throw; // Re-throw the exception to be handled by the caller
            }
        }

        /// <inheritdoc/>
        public async Task<PdfDocument?> GetByTokenAsync(Guid token)
        {
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            
            var accessToken = await _context.PdfAccessTokens
                .Include(t => t.Document)
                .ThenInclude(d => d!.Uploader)
                .FirstOrDefaultAsync(t => t.Token == token && t.ExpiresAt > now && !t.IsRevoked);

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
            // Ensure DateTime values have no Kind specification for timestamp without time zone
            token.CreatedAt = DateTime.SpecifyKind(token.CreatedAt, DateTimeKind.Unspecified);
            token.ExpiresAt = DateTime.SpecifyKind(token.ExpiresAt, DateTimeKind.Unspecified);
            
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
