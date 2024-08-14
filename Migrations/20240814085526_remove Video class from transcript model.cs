using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranskriptTest.Migrations
{
    /// <inheritdoc />
    public partial class removeVideoclassfromtranscriptmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transcripts_Videos_VideoId",
                table: "Transcripts");

            migrationBuilder.DropIndex(
                name: "IX_Transcripts_VideoId",
                table: "Transcripts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transcripts_VideoId",
                table: "Transcripts",
                column: "VideoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transcripts_Videos_VideoId",
                table: "Transcripts",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id");
        }
    }
}
