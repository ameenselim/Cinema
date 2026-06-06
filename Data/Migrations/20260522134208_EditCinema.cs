using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema_Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class EditCinema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RowsCount",
                table: "Cinemas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SeatsPerRow",
                table: "Cinemas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Cinemas",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "RowsCount", "SeatsPerRow" },
                values: new object[] { 10, 10 });

            migrationBuilder.UpdateData(
                table: "Cinemas",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "RowsCount", "SeatsPerRow" },
                values: new object[] { 8, 4 });

            migrationBuilder.UpdateData(
                table: "Cinemas",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "RowsCount", "SeatsPerRow" },
                values: new object[] { 7, 10 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowsCount",
                table: "Cinemas");

            migrationBuilder.DropColumn(
                name: "SeatsPerRow",
                table: "Cinemas");
        }
    }
}
