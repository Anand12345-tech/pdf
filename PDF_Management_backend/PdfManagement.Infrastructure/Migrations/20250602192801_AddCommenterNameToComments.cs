using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PdfManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommenterNameToComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CommenterName",
                table: "PdfComments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommenterName",
                table: "PdfComments");
        }
    }
}
