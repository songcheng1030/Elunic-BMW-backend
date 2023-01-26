using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AIQXFileService.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file__files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file__files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "file__ref_ids",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file__ref_ids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_file__ref_ids_file__files_FileId",
                        column: x => x.FileId,
                        principalTable: "file__files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "file__tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file__tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_file__tags_file__files_FileId",
                        column: x => x.FileId,
                        principalTable: "file__files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_file__ref_ids_FileId",
                table: "file__ref_ids",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_file__tags_FileId",
                table: "file__tags",
                column: "FileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file__ref_ids");

            migrationBuilder.DropTable(
                name: "file__tags");

            migrationBuilder.DropTable(
                name: "file__files");
        }
    }
}
