using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElmanhagPlatform.Migrations
{
    /// <inheritdoc />
    public partial class configTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configs",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.InsertData(
                table: "Configs",
                columns: new[] { "Id", "Url" },
                values: new object[] { 1, "Not Add" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.InsertData(
                table: "Configs",
                columns: new[] { "Id", "Url" },
                values: new object[] { -1, "Not Add" });
        }
    }
}
