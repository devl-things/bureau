using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bureau.Data.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class OccurrenceRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OccurrenceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Frequency = table.Column<int>(type: "integer", nullable: true),
                    Interval = table.Column<int>(type: "integer", nullable: false),
                    ByDay = table.Column<string>(type: "text", nullable: false),
                    ByMonthDay = table.Column<string>(type: "text", nullable: false),
                    ByYearDay = table.Column<string>(type: "text", nullable: false),
                    ByWeekNo = table.Column<string>(type: "text", nullable: false),
                    ByMonth = table.Column<string>(type: "text", nullable: false),
                    BySetPos = table.Column<string>(type: "text", nullable: false),
                    Until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Count = table.Column<int>(type: "integer", nullable: true),
                    WeekStart = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccurrenceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OccurrenceRecords_Records_Id",
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
                    { 12, "Group", "EdgeTypeEnum" },
                    { 90, "Daily", "RecurrenceFrequency" },
                    { 91, "Weekly", "RecurrenceFrequency" },
                    { 92, "Monthly", "RecurrenceFrequency" },
                    { 93, "Yearly", "RecurrenceFrequency" },
                    { 94, "Minutely", "RecurrenceFrequency" },
                    { 95, "Secondly", "RecurrenceFrequency" },
                    { 100, "Recipe", "EdgeTypeEnum" },
                    { 110, "Calendar", "EdgeTypeEnum" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OccurrenceRecords");

            migrationBuilder.DeleteData(
                table: "EnumData",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "EnumData",
                keyColumn: "Id",
                keyValue: 90);

            migrationBuilder.DeleteData(
                table: "EnumData",
                keyColumn: "Id",
                keyValue: 91);

            migrationBuilder.DeleteData(
                table: "EnumData",
                keyColumn: "Id",
                keyValue: 92);

            migrationBuilder.DeleteData(
                table: "EnumData",
                keyColumn: "Id",
                keyValue: 93);

            migrationBuilder.DeleteData(
                table: "EnumData",
                keyColumn: "Id",
                keyValue: 94);

            migrationBuilder.DeleteData(
                table: "EnumData",
                keyColumn: "Id",
                keyValue: 95);

            migrationBuilder.DeleteData(
                table: "EnumData",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "EnumData",
                keyColumn: "Id",
                keyValue: 110);
        }
    }
}
