using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingManagement.Migrations
{
    /// <inheritdoc />
    public partial class _120_Update_UpdateAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "Users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "Payments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "PaymentAllocations",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "Cycles",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "Compounds",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "Buildings",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "Bills",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "Balances",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "Audits",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatededAt",
                table: "Apartments",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Payments",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Users",
                newName: "UpdatededAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Payments",
                newName: "UpdatededAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "PaymentAllocations",
                newName: "UpdatededAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Cycles",
                newName: "UpdatededAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Compounds",
                newName: "UpdatededAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Buildings",
                newName: "UpdatededAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Bills",
                newName: "UpdatededAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Balances",
                newName: "UpdatededAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Audits",
                newName: "UpdatededAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Apartments",
                newName: "UpdatededAt");
        }
    }
}
