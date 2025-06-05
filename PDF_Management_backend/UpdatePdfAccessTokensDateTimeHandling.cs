using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PdfManagement.Infrastructure.Data.Context;
using System;

namespace PdfManagement.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250605050001_UpdatePdfAccessTokensToTimestampWithTimeZone")]
    public partial class UpdatePdfAccessTokensToTimestampWithTimeZone : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration documents the change you've already made manually
            // It ensures the entity model matches the database schema
            
            // Run the ALTER TABLE commands again to ensure the columns are properly set
            migrationBuilder.Sql("ALTER TABLE \"PdfAccessTokens\" ALTER COLUMN \"CreatedAt\" TYPE timestamp WITH time zone");
            migrationBuilder.Sql("ALTER TABLE \"PdfAccessTokens\" ALTER COLUMN \"ExpiresAt\" TYPE timestamp WITH time zone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to timestamp without time zone if needed
            migrationBuilder.Sql("ALTER TABLE \"PdfAccessTokens\" ALTER COLUMN \"CreatedAt\" TYPE timestamp WITHOUT time zone");
            migrationBuilder.Sql("ALTER TABLE \"PdfAccessTokens\" ALTER COLUMN \"ExpiresAt\" TYPE timestamp WITHOUT time zone");
        }
    }
}
