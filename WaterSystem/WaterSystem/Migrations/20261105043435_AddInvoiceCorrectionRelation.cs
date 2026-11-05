using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceCorrectionRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OriginalInvoiceId",
                table: "Invoices",
                column: "OriginalInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Invoices_OriginalInvoiceId",
                table: "Invoices",
                column: "OriginalInvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Invoices_OriginalInvoiceId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_OriginalInvoiceId",
                table: "Invoices");
        }
    }
}
