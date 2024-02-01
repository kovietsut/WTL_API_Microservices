using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContent_MangaInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Chapters",
                type: "varchar(max)",
                unicode: false,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Chapters");
        }
    }
}
