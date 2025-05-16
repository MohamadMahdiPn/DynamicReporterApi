using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicReporterApi.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddSpSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStoredProcedure",
                table: "CustomReports",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsStoredProcedure",
                table: "CustomReports");
        }
    }
}
