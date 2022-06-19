using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kekes.Data.Migrations
{
    public partial class AddedDescriptionIntoSection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Sections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Sections");
        }
    }
}
