using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AIQXCoreService.Migrations
{
    public partial class UpdatedUseCaseAndUseCaseStep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "core__status_updates");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "core__use_case_steps",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "core__use_case_steps");

            migrationBuilder.CreateTable(
                name: "core__status_updates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UseCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core__status_updates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_core__status_updates_core__use_cases_UseCaseId",
                        column: x => x.UseCaseId,
                        principalTable: "core__use_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_core__status_updates_UseCaseId",
                table: "core__status_updates",
                column: "UseCaseId");
        }
    }
}
