using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NehasBeed.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "AdminNotifications",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "AdminNotifications");
        }
    }
}
