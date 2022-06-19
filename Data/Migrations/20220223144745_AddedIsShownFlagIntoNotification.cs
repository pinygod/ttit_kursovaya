using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kekes.Data.Migrations
{
    public partial class AddedIsShownFlagIntoNotification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsShown",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsShown",
                table: "Notifications");
        }
    }
}
