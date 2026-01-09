using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlLibrería.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchaseOrderUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CanceledByUserId",
                table: "PurchaseOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "PurchaseOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceivedByUserId",
                table: "PurchaseOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CanceledByUserId",
                table: "PurchaseOrders",
                column: "CanceledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_CreatedByUserId",
                table: "PurchaseOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_ReceivedByUserId",
                table: "PurchaseOrders",
                column: "ReceivedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_AspNetUsers_CanceledByUserId",
                table: "PurchaseOrders",
                column: "CanceledByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_AspNetUsers_CreatedByUserId",
                table: "PurchaseOrders",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_AspNetUsers_ReceivedByUserId",
                table: "PurchaseOrders",
                column: "ReceivedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_AspNetUsers_CanceledByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_AspNetUsers_CreatedByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_AspNetUsers_ReceivedByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_CanceledByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_CreatedByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_ReceivedByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CanceledByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ReceivedByUserId",
                table: "PurchaseOrders");
        }
    }
}
