using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PdfManagement.Core.Domain.Entities;
using System;

namespace PdfManagement.Infrastructure.Data.Context
{
    /// <summary>
    /// Application database context
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PdfDocument> PdfDocuments { get; set; }
        public DbSet<PdfComment> PdfComments { get; set; }
        public DbSet<PdfAccessToken> PdfAccessTokens { get; set; }
        public DbSet<PdfAccessLog> PdfAccessLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure DateTime properties for PdfAccessToken to use timestamp without time zone
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Unspecified)
            );

            builder.Entity<PdfAccessToken>()
                .Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasConversion(dateTimeConverter);

            builder.Entity<PdfAccessToken>()
                .Property(e => e.ExpiresAt)
                .HasColumnType("timestamp without time zone")
                .HasConversion(dateTimeConverter);

            // Configure PdfDocument entity
            builder.Entity<PdfDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UploaderId).IsRequired();
                entity.Property(e => e.UploadedAt).IsRequired();

                entity.HasOne(e => e.Uploader)
                    .WithMany(u => u.Documents)
                    .HasForeignKey(e => e.UploaderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PdfComment entity
            builder.Entity<PdfComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.PageNumber).IsRequired();
                entity.Property(e => e.UserType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(e => e.Document)
                    .WithMany(d => d.Comments)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Commenter)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(e => e.CommenterId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure PdfAccessToken entity
            builder.Entity<PdfAccessToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired();
                entity.Property(e => e.IsRevoked).IsRequired();

                entity.HasOne(e => e.Document)
                    .WithMany(d => d.AccessTokens)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token).IsUnique();
            });

            // Configure PdfAccessLog entity
            builder.Entity<PdfAccessLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AccessedAt).IsRequired();

                entity.HasOne(e => e.Document)
                    .WithMany(d => d.AccessLogs)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
