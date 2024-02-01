using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChapter_MangaInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChapterId",
                table: "MangaInteractions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MangaInteractions_ChapterId",
                table: "MangaInteractions",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MangaInteractions_Chapters_ChapterId",
                table: "MangaInteractions",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MangaInteractions_Chapters_ChapterId",
                table: "MangaInteractions");

            migrationBuilder.DropIndex(
                name: "IX_MangaInteractions_ChapterId",
                table: "MangaInteractions");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "MangaInteractions");
        }
    }
}
