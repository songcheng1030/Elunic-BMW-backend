using Microsoft.EntityFrameworkCore.Migrations;

namespace AIQXCoreService.Migrations
{
    public partial class SplitFormSteps : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [core__use_case_steps]", true);
            migrationBuilder.Sql("DELETE FROM [core__use_cases]", true);

            migrationBuilder.DropColumn(
                name: "Form",
                table: "core__use_cases");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "core__use_case_steps",
                newName: "Form");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "core__use_case_steps",
                newName: "CompletedAt");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "core__use_case_steps",
                type: "int",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "core__use_case_steps");

            migrationBuilder.RenameColumn(
                name: "Form",
                table: "core__use_case_steps",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "core__use_case_steps",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Form",
                table: "core__use_cases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
