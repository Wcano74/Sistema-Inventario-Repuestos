using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlLibrería.Migrations
{
    /// <inheritdoc />
    public partial class SetExistingUsersActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE AspNetUsers SET IsActive = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
