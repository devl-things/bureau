using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sven.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RedirectUris = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ClientUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Contacts = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GrantTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Jwks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JwksUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    LogoUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PolicyUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ResponseTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoftwareId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoftwareVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AuthMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TosUri = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IdTokenLifetime = table.Column<long>(type: "bigint", nullable: true),
                    RefreshTokenLifetime = table.Column<long>(type: "bigint", nullable: true),
                    AccessTokenLifetime = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Identifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Identifier",
                table: "Clients",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Identifier",
                table: "Users",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
