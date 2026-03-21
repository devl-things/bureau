using Microsoft.EntityFrameworkCore.Migrations;

namespace Sven.Data.Postgres.Migrations
{
    public partial class AddClientHashedSecret : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashedSecret",
                table: "Clients",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashedSecret",
                table: "Clients");
        }
    }
}
