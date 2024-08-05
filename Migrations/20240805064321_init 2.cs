using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TranskriptTest.Migrations
{
    /// <inheritdoc />
    public partial class init2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubtitleRequests_Subtitles_SubtitleId",
                table: "SubtitleRequests");

            migrationBuilder.AlterColumn<int>(
                name: "SubtitleId",
                table: "SubtitleRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_SubtitleRequests_Subtitles_SubtitleId",
                table: "SubtitleRequests",
                column: "SubtitleId",
                principalTable: "Subtitles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubtitleRequests_Subtitles_SubtitleId",
                table: "SubtitleRequests");

            migrationBuilder.AlterColumn<int>(
                name: "SubtitleId",
                table: "SubtitleRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubtitleRequests_Subtitles_SubtitleId",
                table: "SubtitleRequests",
                column: "SubtitleId",
                principalTable: "Subtitles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
