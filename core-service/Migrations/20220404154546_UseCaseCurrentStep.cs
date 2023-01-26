using Microsoft.EntityFrameworkCore.Migrations;

namespace AIQXCoreService.Migrations
{
    public partial class UseCaseCurrentStep : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentStep",
                table: "core__use_cases",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // MIgrate status away from step to enabled/declined.
            migrationBuilder.Sql("UPDATE dbo.core__use_cases SET Status = (CASE Status WHEN 5 THEN 2 ELSE 1 END)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStep",
                table: "core__use_cases");
        }
    }
}
