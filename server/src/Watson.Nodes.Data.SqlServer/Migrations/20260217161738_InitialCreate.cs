using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watson.Nodes.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Nodes",
                columns: table => new
                {
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedSequence = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kind = table.Column<byte>(type: "tinyint", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CanonicalKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nodes", x => x.NodeId);
                });

            migrationBuilder.CreateTable(
                name: "NodeAttributes",
                columns: table => new
                {
                    NodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false, defaultValue: "default"),
                    Type = table.Column<byte>(type: "tinyint", nullable: false),
                    ValueString = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueNumber = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ValueBool = table.Column<bool>(type: "bit", nullable: true),
                    ValueJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ValueDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeAttributes", x => new { x.NodeId, x.Key, x.Locale });
                    table.ForeignKey(
                        name: "FK_NodeAttributes_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodeEdges",
                columns: table => new
                {
                    SourceNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetNodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodeEdges", x => new { x.SourceNodeId, x.TargetNodeId, x.Purpose });
                    table.ForeignKey(
                        name: "FK_NodeEdges_Nodes_SourceNodeId",
                        column: x => x.SourceNodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NodeEdges_Nodes_TargetNodeId",
                        column: x => x.TargetNodeId,
                        principalTable: "Nodes",
                        principalColumn: "NodeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NodeAttributes_Key_Locale_RefNodeId",
                table: "NodeAttributes",
                columns: new[] { "Key", "Locale", "RefNodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeAttributes_Key_Locale_Type_NodeId",
                table: "NodeAttributes",
                columns: new[] { "Key", "Locale", "Type", "NodeId" })
                .Annotation("SqlServer:Include", new[] { "ValueString", "ValueJson" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeAttributes_Key_Locale_ValueBool",
                table: "NodeAttributes",
                columns: new[] { "Key", "Locale", "ValueBool" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeAttributes_Key_Locale_ValueDate",
                table: "NodeAttributes",
                columns: new[] { "Key", "Locale", "ValueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeAttributes_Key_Locale_ValueNumber",
                table: "NodeAttributes",
                columns: new[] { "Key", "Locale", "ValueNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeAttributes_NodeId_Key_Locale",
                table: "NodeAttributes",
                columns: new[] { "NodeId", "Key", "Locale" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeAttributes_NodeId_Key_Locale_OrderIndex",
                table: "NodeAttributes",
                columns: new[] { "NodeId", "Key", "Locale", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeEdges_SourceNodeId_Purpose",
                table: "NodeEdges",
                columns: new[] { "SourceNodeId", "Purpose" });

            migrationBuilder.CreateIndex(
                name: "IX_NodeEdges_TargetNodeId_Purpose",
                table: "NodeEdges",
                columns: new[] { "TargetNodeId", "Purpose" });

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_Kind_Scope_CanonicalKey",
                table: "Nodes",
                columns: new[] { "Kind", "Scope", "CanonicalKey" },
                unique: true,
                filter: "[CanonicalKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Nodes_Kind_Scope_CreatedSequence",
                table: "Nodes",
                columns: new[] { "Kind", "Scope", "CreatedSequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NodeAttributes");

            migrationBuilder.DropTable(
                name: "NodeEdges");

            migrationBuilder.DropTable(
                name: "Nodes");
        }
    }
}
