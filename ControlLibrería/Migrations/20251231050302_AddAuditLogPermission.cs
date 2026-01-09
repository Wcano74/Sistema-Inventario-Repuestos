using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlLibrería.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanViewAuditLog",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanViewAuditLog",
                table: "AspNetUsers");
        }
    }
}
