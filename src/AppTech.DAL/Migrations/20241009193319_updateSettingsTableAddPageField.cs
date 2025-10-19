using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTech.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateSettingsTableAddPageField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Page",
                table: "Settings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Page",
                table: "Settings");
        }
    }
}
