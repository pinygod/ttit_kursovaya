using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kekes.Data.Migrations
{
    public partial class UserTagsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserTagsId",
                table: "Tags",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTags_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserTagsId",
                table: "Tags",
                column: "UserTagsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTags_UserId",
                table: "UserTags",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_UserTags_UserTagsId",
                table: "Tags",
                column: "UserTagsId",
                principalTable: "UserTags",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_UserTags_UserTagsId",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "UserTags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserTagsId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UserTagsId",
                table: "Tags");
        }
    }
}
