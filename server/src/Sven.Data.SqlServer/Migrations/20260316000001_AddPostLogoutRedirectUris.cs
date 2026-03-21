using Microsoft.EntityFrameworkCore.Migrations;

namespace Sven.Data.SqlServer.Migrations
{
    public partial class AddPostLogoutRedirectUris : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostLogoutRedirectUris",
                table: "Clients",
                type: "nvarchar(max)",
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
