using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMetering.Billing.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeterId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PeriodEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meter_balances",
                columns: table => new
                {
                    MeterId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    AccumulatedKwh = table.Column<decimal>(type: "numeric(14,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meter_balances", x => x.MeterId);
                });

            migrationBuilder.CreateTable(
                name: "line_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(14,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(14,4)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_line_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_line_items_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_line_items_InvoiceId",
                table: "line_items",
                column: "InvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "line_items");

            migrationBuilder.DropTable(
                name: "meter_balances");

            migrationBuilder.DropTable(
                name: "invoices");
        }
    }
}
