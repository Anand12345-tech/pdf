using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PdfManagement.Data.Models;

namespace PdfManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PdfDocument> PdfDocuments { get; set; }
        public DbSet<PdfAccessToken> PdfAccessTokens { get; set; }
        public DbSet<PdfAccessLog> PdfAccessLogs { get; set; }
        public DbSet<PdfComment> PdfComments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<PdfDocument>()
                .HasOne(d => d.Uploader)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(d => d.UploaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PdfAccessToken>()
                .HasOne(t => t.PdfDocument)
                .WithMany(d => d.AccessTokens)
                .HasForeignKey(t => t.PdfDocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PdfAccessLog>()
                .HasOne(l => l.AccessToken)
                .WithMany(t => t.AccessLogs)
                .HasForeignKey(l => l.AccessTokenId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PdfComment>()
                .HasOne(c => c.PdfDocument)
                .WithMany(d => d.Comments)
                .HasForeignKey(c => c.PdfDocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PdfComment>()
                .HasOne(c => c.Commenter)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.CommenterId)
                .OnDelete(DeleteBehavior.NoAction);

            // Self-referencing relationship for nested comments
            builder.Entity<PdfComment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
