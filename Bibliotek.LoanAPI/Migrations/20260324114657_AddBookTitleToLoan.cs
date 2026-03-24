using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bibliotek.LoanAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddBookTitleToLoan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BookTitle",
                table: "Loans",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookTitle",
                table: "Loans");
        }
    }
}
