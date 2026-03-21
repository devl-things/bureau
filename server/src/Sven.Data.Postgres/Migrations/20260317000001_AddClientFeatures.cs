using Microsoft.EntityFrameworkCore.Migrations;

namespace Sven.Data.Postgres.Migrations
{
    public partial class AddClientFeatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientFeatures",
                columns: table => new
                {
                    ClientId = table.Column<int>(nullable: false),
                    FeatureKey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientFeatures", x => new { x.ClientId, x.FeatureKey });
                    table.ForeignKey(
                        name: "FK_ClientFeatures_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientFeatureExternalRequirements",
                columns: table => new
                {
                    ClientId = table.Column<int>(nullable: false),
                    FeatureKey = table.Column<string>(type: "text", nullable: false),
                    ExternalScopeKey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientFeatureExternalRequirements", x => new { x.ClientId, x.FeatureKey, x.ExternalScopeKey });
                    table.ForeignKey(
                        name: "FK_ClientFeatureExternalRequirements_ClientFeatures_ClientId_FeatureKey",
                        columns: x => new { x.ClientId, x.FeatureKey },
                        principalTable: "ClientFeatures",
                        principalColumns: new[] { "ClientId", "FeatureKey" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ClientFeatureExternalRequirements");
            migrationBuilder.DropTable(name: "ClientFeatures");
        }
    }
}
