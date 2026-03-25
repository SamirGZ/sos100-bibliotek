using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationsService.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameAndIsRead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Notifications",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "Notifications");
        }
    }
}
