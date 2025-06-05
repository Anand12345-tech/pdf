using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PdfManagement.Infrastructure.Data.Context;
using System;

namespace PdfManagement.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250605050000_UpdatePdfAccessTokensDateTimeHandling")]
    public partial class UpdatePdfAccessTokensDateTimeHandling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration assumes you've already run the ALTER TABLE command manually
            // This is just to document the change in your migration history
            
            // Run the ALTER TABLE commands again to ensure the columns are properly set
            migrationBuilder.Sql("ALTER TABLE \"PdfAccessTokens\" ALTER COLUMN \"CreatedAt\" TYPE timestamp WITHOUT time zone");
            migrationBuilder.Sql("ALTER TABLE \"PdfAccessTokens\" ALTER COLUMN \"ExpiresAt\" TYPE timestamp WITHOUT time zone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to timestamp with time zone if needed
            migrationBuilder.Sql("ALTER TABLE \"PdfAccessTokens\" ALTER COLUMN \"CreatedAt\" TYPE timestamp WITH time zone");
            migrationBuilder.Sql("ALTER TABLE \"PdfAccessTokens\" ALTER COLUMN \"ExpiresAt\" TYPE timestamp WITH time zone");
        }
    }
}
