using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manga.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModificationOfAlbum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Albums");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Albums",
                newName: "ModifiedAt");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Albums",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "Albums",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ModifiedBy",
                table: "Albums",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Albums");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "Albums",
                newName: "CreatedDate");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "Albums",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
