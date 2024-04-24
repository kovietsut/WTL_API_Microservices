using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlbum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CoverImage = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlbumsMangas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MangaId = table.Column<long>(type: "bigint", nullable: false),
                    AlbumId = table.Column<long>(type: "bigint", nullable: false),
                    AddedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumsMangas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumsMangas_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumsMangas_Mangas_MangaId",
                        column: x => x.MangaId,
                        principalTable: "Mangas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumsMangas_AlbumId",
                table: "AlbumsMangas",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumsMangas_MangaId",
                table: "AlbumsMangas",
                column: "MangaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumsMangas");

            migrationBuilder.DropTable(
                name: "Albums");
        }
    }
}
