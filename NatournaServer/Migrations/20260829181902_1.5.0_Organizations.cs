using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NatournaServer.Migrations
{
    /// <inheritdoc />
    public partial class _150_Organizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "PaymentAllocations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Cycles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Compounds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Buildings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Bills",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Balances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Audits",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrganizationId",
                table: "Apartments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LbpExchangeRate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PricePerBuilding = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ================================================================
            // BACKFILL: adopt pre-existing single-tenant data into one default
            // organization (named after the first compound) so the foreign keys
            // added below hold. A fresh, empty database passes through untouched.
            // ================================================================
            migrationBuilder.Sql(@"
                INSERT INTO ""Organizations"" (""Name"", ""IsActive"", ""CreatedAt"", ""UpdatedAt"")
                SELECT COALESCE((SELECT ""Name"" FROM ""Compounds"" ORDER BY ""Id"" LIMIT 1), 'Default Organization'), TRUE, NOW(), NOW()
                WHERE EXISTS (SELECT 1 FROM ""Compounds"") OR EXISTS (SELECT 1 FROM ""Users"");

                INSERT INTO ""Subscriptions"" (""OrganizationId"", ""Status"", ""PricePerBuilding"", ""StartDate"", ""CreatedAt"", ""UpdatedAt"")
                SELECT o.""Id"", 1, 7.00, NOW(), NOW(), NOW()
                FROM ""Organizations"" o
                ORDER BY o.""Id"" LIMIT 1;

                UPDATE ""Compounds""          SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" = 0 AND EXISTS (SELECT 1 FROM ""Organizations"");
                UPDATE ""Buildings""          SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" = 0 AND EXISTS (SELECT 1 FROM ""Organizations"");
                UPDATE ""Apartments""         SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" = 0 AND EXISTS (SELECT 1 FROM ""Organizations"");
                UPDATE ""Balances""           SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" = 0 AND EXISTS (SELECT 1 FROM ""Organizations"");
                UPDATE ""Bills""              SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" = 0 AND EXISTS (SELECT 1 FROM ""Organizations"");
                UPDATE ""Payments""           SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" = 0 AND EXISTS (SELECT 1 FROM ""Organizations"");
                UPDATE ""PaymentAllocations"" SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" = 0 AND EXISTS (SELECT 1 FROM ""Organizations"");
                UPDATE ""Cycles""             SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" = 0 AND EXISTS (SELECT 1 FROM ""Organizations"");
                UPDATE ""Users""              SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" = 0 AND EXISTS (SELECT 1 FROM ""Organizations"");
                UPDATE ""Audits""             SET ""OrganizationId"" = (SELECT MIN(""Id"") FROM ""Organizations"") WHERE ""OrganizationId"" IS NULL AND EXISTS (SELECT 1 FROM ""Organizations"");
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Users_OrganizationId",
                table: "Users",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrganizationId",
                table: "Payments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_OrganizationId",
                table: "PaymentAllocations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Cycles_OrganizationId",
                table: "Cycles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Compounds_OrganizationId",
                table: "Compounds",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_OrganizationId",
                table: "Buildings",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bills_OrganizationId",
                table: "Bills",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Balances_OrganizationId",
                table: "Balances",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Audits_OrganizationId",
                table: "Audits",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Apartments_OrganizationId",
                table: "Apartments",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_OrganizationId",
                table: "Subscriptions",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Apartments_Organizations_OrganizationId",
                table: "Apartments",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Balances_Organizations_OrganizationId",
                table: "Balances",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Bills_Organizations_OrganizationId",
                table: "Bills",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Organizations_OrganizationId",
                table: "Buildings",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Compounds_Organizations_OrganizationId",
                table: "Compounds",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cycles_Organizations_OrganizationId",
                table: "Cycles",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentAllocations_Organizations_OrganizationId",
                table: "PaymentAllocations",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Organizations_OrganizationId",
                table: "Payments",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Apartments_Organizations_OrganizationId",
                table: "Apartments");

            migrationBuilder.DropForeignKey(
                name: "FK_Balances_Organizations_OrganizationId",
                table: "Balances");

            migrationBuilder.DropForeignKey(
                name: "FK_Bills_Organizations_OrganizationId",
                table: "Bills");

            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Organizations_OrganizationId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_Compounds_Organizations_OrganizationId",
                table: "Compounds");

            migrationBuilder.DropForeignKey(
                name: "FK_Cycles_Organizations_OrganizationId",
                table: "Cycles");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentAllocations_Organizations_OrganizationId",
                table: "PaymentAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Organizations_OrganizationId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Organizations_OrganizationId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Users_OrganizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Payments_OrganizationId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_PaymentAllocations_OrganizationId",
                table: "PaymentAllocations");

            migrationBuilder.DropIndex(
                name: "IX_Cycles_OrganizationId",
                table: "Cycles");

            migrationBuilder.DropIndex(
                name: "IX_Compounds_OrganizationId",
                table: "Compounds");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_OrganizationId",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Bills_OrganizationId",
                table: "Bills");

            migrationBuilder.DropIndex(
                name: "IX_Balances_OrganizationId",
                table: "Balances");

            migrationBuilder.DropIndex(
                name: "IX_Audits_OrganizationId",
                table: "Audits");

            migrationBuilder.DropIndex(
                name: "IX_Apartments_OrganizationId",
                table: "Apartments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "PaymentAllocations");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Cycles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Compounds");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Bills");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Balances");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Audits");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Apartments");
        }
    }
}
