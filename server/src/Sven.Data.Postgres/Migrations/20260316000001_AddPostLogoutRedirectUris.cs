using Microsoft.EntityFrameworkCore.Migrations;

namespace Sven.Data.Postgres.Migrations
{
    public partial class AddPostLogoutRedirectUris : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostLogoutRedirectUris",
                table: "Clients",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostLogoutRedirectUris",
                table: "Clients");
        }
    }
}
