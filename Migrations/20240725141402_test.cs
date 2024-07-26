using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranskriptTest.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subtitles_VideoId",
                table: "Subtitles");

            migrationBuilder.CreateIndex(
                name: "IX_Subtitles_VideoId",
                table: "Subtitles",
                column: "VideoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subtitles_VideoId",
                table: "Subtitles");

            migrationBuilder.CreateIndex(
                name: "IX_Subtitles_VideoId",
                table: "Subtitles",
                column: "VideoId");
        }
    }
}
