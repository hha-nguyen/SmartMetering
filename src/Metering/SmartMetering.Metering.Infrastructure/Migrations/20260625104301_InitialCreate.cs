using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartMetering.Metering.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "readings",
                columns: table => new
                {
                    meter_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    kwh = table.Column<decimal>(type: "numeric(12,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_readings", x => new { x.meter_id, x.timestamp });
                });

            // TimescaleDB: biến bảng thường thành hypertable phân vùng theo timestamp
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS timescaledb;");
            migrationBuilder.Sql("SELECT create_hypertable('readings', 'timestamp', if_not_exists => TRUE);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "readings");
        }
    }
}
