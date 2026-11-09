using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToMeterRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "MeterRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeterRequests_UserId",
                table: "MeterRequests",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MeterRequests_AspNetUsers_UserId",
                table: "MeterRequests",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MeterRequests_AspNetUsers_UserId",
                table: "MeterRequests");

            migrationBuilder.DropIndex(
                name: "IX_MeterRequests_UserId",
                table: "MeterRequests");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MeterRequests");
        }
    }
}
