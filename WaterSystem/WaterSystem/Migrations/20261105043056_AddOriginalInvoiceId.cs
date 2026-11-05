using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalInvoiceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalInvoiceId",
                table: "Invoices",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalInvoiceId",
                table: "Invoices");
        }
    }
}
