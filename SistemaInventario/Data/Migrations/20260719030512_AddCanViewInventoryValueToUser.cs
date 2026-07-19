using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaInventario.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCanViewInventoryValueToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanViewInventoryValue",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanViewInventoryValue",
                table: "AspNetUsers");
        }
    }
}
