using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AIQXCoreService.Migrations
{
    public partial class UseCaseAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UseCaseId",
                table: "core__attachments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_core__attachments_UseCaseId",
                table: "core__attachments",
                column: "UseCaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_core__attachments_core__use_cases_UseCaseId",
                table: "core__attachments",
                column: "UseCaseId",
                principalTable: "core__use_cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_core__attachments_core__use_cases_UseCaseId",
                table: "core__attachments");

            migrationBuilder.DropIndex(
                name: "IX_core__attachments_UseCaseId",
                table: "core__attachments");

            migrationBuilder.DropColumn(
                name: "UseCaseId",
                table: "core__attachments");
        }
    }
}
