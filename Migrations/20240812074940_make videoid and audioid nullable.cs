using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranskriptTest.Migrations
{
    /// <inheritdoc />
    public partial class makevideoidandaudioidnullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subtitles_AudioFiles_AudioFileId",
                table: "Subtitles");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtitles_Videos_VideoId",
                table: "Subtitles");

            migrationBuilder.AlterColumn<int>(
                name: "VideoId",
                table: "Subtitles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AudioFileId",
                table: "Subtitles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Subtitles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitles_AudioFiles_AudioFileId",
                table: "Subtitles",
                column: "AudioFileId",
                principalTable: "AudioFiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitles_Videos_VideoId",
                table: "Subtitles",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subtitles_AudioFiles_AudioFileId",
                table: "Subtitles");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtitles_Videos_VideoId",
                table: "Subtitles");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Subtitles");

            migrationBuilder.AlterColumn<int>(
                name: "VideoId",
                table: "Subtitles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AudioFileId",
                table: "Subtitles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitles_AudioFiles_AudioFileId",
                table: "Subtitles",
                column: "AudioFileId",
                principalTable: "AudioFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitles_Videos_VideoId",
                table: "Subtitles",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
