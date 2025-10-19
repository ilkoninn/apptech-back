using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTech.DAL.Migrations
{
    /// <inheritdoc />
    public partial class updateSshKeyPairTableAndAddNewPublicIpAddressTablev2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SshKeyPairs_ExamResults_ExamResultId",
                table: "SshKeyPairs");

            migrationBuilder.DropIndex(
                name: "IX_SshKeyPairs_ExamResultId",
                table: "SshKeyPairs");

            migrationBuilder.AlterColumn<int>(
                name: "ExamResultId",
                table: "SshKeyPairs",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_SshKeyPairs_ExamResultId",
                table: "SshKeyPairs",
                column: "ExamResultId",
                unique: true,
                filter: "[ExamResultId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SshKeyPairs_ExamResults_ExamResultId",
                table: "SshKeyPairs",
                column: "ExamResultId",
                principalTable: "ExamResults",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SshKeyPairs_ExamResults_ExamResultId",
                table: "SshKeyPairs");

            migrationBuilder.DropIndex(
                name: "IX_SshKeyPairs_ExamResultId",
                table: "SshKeyPairs");

            migrationBuilder.AlterColumn<int>(
                name: "ExamResultId",
                table: "SshKeyPairs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SshKeyPairs_ExamResultId",
                table: "SshKeyPairs",
                column: "ExamResultId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SshKeyPairs_ExamResults_ExamResultId",
                table: "SshKeyPairs",
                column: "ExamResultId",
                principalTable: "ExamResults",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
