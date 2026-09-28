using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterSystem.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consumptions_Meters_MeterId",
                table: "Consumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Meters_AspNetUsers_UserId",
                table: "Meters");

            migrationBuilder.AddForeignKey(
                name: "FK_Consumptions_Meters_MeterId",
                table: "Consumptions",
                column: "MeterId",
                principalTable: "Meters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Meters_AspNetUsers_UserId",
                table: "Meters",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consumptions_Meters_MeterId",
                table: "Consumptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Meters_AspNetUsers_UserId",
                table: "Meters");

            migrationBuilder.AddForeignKey(
                name: "FK_Consumptions_Meters_MeterId",
                table: "Consumptions",
                column: "MeterId",
                principalTable: "Meters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Meters_AspNetUsers_UserId",
                table: "Meters",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
