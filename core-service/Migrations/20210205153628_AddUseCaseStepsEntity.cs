using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AIQXCoreService.Migrations
{
    public partial class AddUseCaseStepsEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedSteps",
                table: "core__use_cases");

            migrationBuilder.CreateTable(
                name: "core__use_case_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseCaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core__use_case_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_core__use_case_steps_core__use_cases_UseCaseId",
                        column: x => x.UseCaseId,
                        principalTable: "core__use_cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_core__use_case_steps_UseCaseId",
                table: "core__use_case_steps",
                column: "UseCaseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "core__use_case_steps");

            migrationBuilder.AddColumn<string>(
                name: "CompletedSteps",
                table: "core__use_cases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
