using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumptionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Consumptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Consumptions");
        }
    }
}
