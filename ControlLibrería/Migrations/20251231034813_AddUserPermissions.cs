using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlLibrería.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanAccessPurchases",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanCreateOrders",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageOrders",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanAccessPurchases",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CanCreateOrders",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CanManageOrders",
                table: "AspNetUsers");
        }
    }
}
