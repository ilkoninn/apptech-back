using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTech.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateDroptletTableAddNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjectName",
                table: "Droplets",
                newName: "VpcId");

            migrationBuilder.RenameColumn(
                name: "DropletId",
                table: "Droplets",
                newName: "MachineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VpcId",
                table: "Droplets",
                newName: "ProjectName");

            migrationBuilder.RenameColumn(
                name: "MachineId",
                table: "Droplets",
                newName: "DropletId");
        }
    }
}
