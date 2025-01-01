using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bureau.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnumData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    EnumType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProviderName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Edges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EdgeType = table.Column<int>(type: "integer", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    ParentNodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RootNodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Edges_Records_Id",
                        column: x => x.Id,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Edges_Records_ParentNodeId",
                        column: x => x.ParentNodeId,
                        principalTable: "Records",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Edges_Records_RootNodeId",
                        column: x => x.RootNodeId,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Edges_Records_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Edges_Records_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlexibleRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DataType = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlexibleRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlexibleRecords_Records_Id",
                        column: x => x.Id,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TermEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermEntries_Records_Id",
                        column: x => x.Id,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EnumData",
                columns: new[] { "Id", "Description", "EnumType" },
                values: new object[,]
                {
                    { 1, "Draft", "RecordStatus" },
                    { 2, "New", "RecordStatus" },
                    { 3, "Active", "RecordStatus" },
                    { 4, "Archived", "RecordStatus" },
                    { 5, "Deleted", "RecordStatus" },
                    { 10, "Details", "EdgeTypeEnum" },
                    { 11, "Items", "EdgeTypeEnum" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Edges_ParentNodeId",
                table: "Edges",
                column: "ParentNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_RootNodeId",
                table: "Edges",
                column: "RootNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_SourceNodeId",
                table: "Edges",
                column: "SourceNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_TargetNodeId",
                table: "Edges",
                column: "TargetNodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Edges");

            migrationBuilder.DropTable(
                name: "EnumData");

            migrationBuilder.DropTable(
                name: "FlexibleRecords");

            migrationBuilder.DropTable(
                name: "TermEntries");

            migrationBuilder.DropTable(
                name: "Records");
        }
    }
}
