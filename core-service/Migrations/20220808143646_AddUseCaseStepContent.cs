using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIQXCoreService.Migrations
{
    public partial class AddUseCaseStepContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "core__use_case_step_content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseCaseStepId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content_EN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content_DE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_core__use_case_step_content", x => x.Id);
                    table.ForeignKey(
                        name: "FK_core__use_case_step_content_core__use_case_steps_UseCaseStepId",
                        column: x => x.UseCaseStepId,
                        principalTable: "core__use_case_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_core__use_case_step_content_UseCaseStepId",
                table: "core__use_case_step_content",
                column: "UseCaseStepId");

            migrationBuilder.Sql(
                    sql: "CREATE FULLTEXT CATALOG ft AS DEFAULT;",
                    suppressTransaction: true);

            migrationBuilder.Sql(
                sql: "CREATE FULLTEXT INDEX ON core__use_case_step_content(Content_EN Language 1033, Content_DE Language 1031) KEY INDEX PK_core__use_case_step_content ON ft;",
                suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "core__use_case_step_content");
        }
    }
}
