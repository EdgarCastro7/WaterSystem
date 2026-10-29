using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMeterRequestNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "MeterRequests",
                newName: "LastName");

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "MeterRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "MeterRequests");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "MeterRequests",
                newName: "Name");
        }
    }
}
