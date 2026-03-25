using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bibliotek.LoanAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLoanWithHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookId",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "LoanEvents",
                newName: "Action");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Loans",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "Loans");

            migrationBuilder.RenameColumn(
                name: "Action",
                table: "LoanEvents",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "Loans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
