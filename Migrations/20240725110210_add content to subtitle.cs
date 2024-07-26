using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranskriptTest.Migrations
{
    /// <inheritdoc />
    public partial class addcontenttosubtitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Subtitles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "VideoId",
                table: "Subtitles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Subtitles_VideoId",
                table: "Subtitles",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitles_Videos_VideoId",
                table: "Subtitles",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subtitles_Videos_VideoId",
                table: "Subtitles");

            migrationBuilder.DropIndex(
                name: "IX_Subtitles_VideoId",
                table: "Subtitles");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Subtitles");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "Subtitles");
        }
    }
}
