using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cinema_Project.Data.Migrations
{
    /// <inheritdoc />
    public partial class editMovieAndCinema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShowTimes_Cinemas_CinemaId1",
                table: "ShowTimes");

            migrationBuilder.DropForeignKey(
                name: "FK_ShowTimes_Movies_MovieId1",
                table: "ShowTimes");

            migrationBuilder.DropIndex(
                name: "IX_ShowTimes_CinemaId1",
                table: "ShowTimes");

            migrationBuilder.DropIndex(
                name: "IX_ShowTimes_MovieId1",
                table: "ShowTimes");

            migrationBuilder.DropColumn(
                name: "CinemaId1",
                table: "ShowTimes");

            migrationBuilder.DropColumn(
                name: "MovieId1",
                table: "ShowTimes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CinemaId1",
                table: "ShowTimes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MovieId1",
                table: "ShowTimes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShowTimes_CinemaId1",
                table: "ShowTimes",
                column: "CinemaId1");

            migrationBuilder.CreateIndex(
                name: "IX_ShowTimes_MovieId1",
                table: "ShowTimes",
                column: "MovieId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ShowTimes_Cinemas_CinemaId1",
                table: "ShowTimes",
                column: "CinemaId1",
                principalTable: "Cinemas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShowTimes_Movies_MovieId1",
                table: "ShowTimes",
                column: "MovieId1",
                principalTable: "Movies",
                principalColumn: "Id");
        }
    }
}
