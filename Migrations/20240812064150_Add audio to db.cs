using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranskriptTest.Migrations
{
    /// <inheritdoc />
    public partial class Addaudiotodb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AudioFileId",
                table: "Subtitles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AudioFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FilePath = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FileSize = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioFiles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Subtitles_AudioFileId",
                table: "Subtitles",
                column: "AudioFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtitles_AudioFiles_AudioFileId",
                table: "Subtitles",
                column: "AudioFileId",
                principalTable: "AudioFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subtitles_AudioFiles_AudioFileId",
                table: "Subtitles");

            migrationBuilder.DropTable(
                name: "AudioFiles");

            migrationBuilder.DropIndex(
                name: "IX_Subtitles_AudioFileId",
                table: "Subtitles");

            migrationBuilder.DropColumn(
                name: "AudioFileId",
                table: "Subtitles");
        }
    }
}
