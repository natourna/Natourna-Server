using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingManagement.Migrations
{
    /// <inheritdoc />
    public partial class CleanupOrphanedRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apartments_Users_UserEntityId",
                table: "Apartments");

            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Compounds_CompoundId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Bills_CompoundId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Apartments_UserEntityId",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "CompoundId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "UserEntityId",
                table: "Apartments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompoundId",
                table: "Bills",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserEntityId",
                table: "Apartments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bills_CompoundId",
                table: "Bills",
                column: "CompoundId");

            migrationBuilder.CreateIndex(
                name: "IX_Apartments_UserEntityId",
                table: "Apartments",
                column: "UserEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Apartments_Users_UserEntityId",
                table: "Apartments",
                column: "UserEntityId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Compounds_CompoundId",
                table: "Bills",
                column: "CompoundId",
                principalTable: "Compounds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
