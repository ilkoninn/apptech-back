using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTech.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateDropletTableAndAddNewField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublicIpAddresses",
                table: "Droplets",
                newName: "PublicIpAddress");

            migrationBuilder.AddColumn<string>(
                name: "DropletId",
                table: "Droplets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DropletId",
                table: "Droplets");

            migrationBuilder.RenameColumn(
                name: "PublicIpAddress",
                table: "Droplets",
                newName: "PublicIpAddresses");
        }
    }
}
