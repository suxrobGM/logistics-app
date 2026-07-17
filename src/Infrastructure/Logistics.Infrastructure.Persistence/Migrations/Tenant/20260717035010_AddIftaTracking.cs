using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddIftaTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ifta_quarter_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    quarter = table.Column<int>(type: "integer", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    report_json = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ifta_quarter_snapshots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "truck_jurisdiction_mileage",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    miles = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    jurisdiction_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    jurisdiction_region = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_truck_jurisdiction_mileage", x => x.id);
                    table.ForeignKey(
                        name: "fk_truck_jurisdiction_mileage_trucks_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "truck_location_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source = table.Column<string>(type: "text", nullable: true),
                    odometer_reading = table.Column<int>(type: "integer", nullable: true),
                    location_latitude = table.Column<double>(type: "double precision", nullable: false),
                    location_longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_truck_location_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_truck_location_history_trucks_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ifta_quarter_snapshots_year_quarter",
                table: "ifta_quarter_snapshots",
                columns: new[] { "year", "quarter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_truck_jurisdiction_mileage_date",
                table: "truck_jurisdiction_mileage",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_truck_jurisdiction_mileage_truck_id_date",
                table: "truck_jurisdiction_mileage",
                columns: new[] { "truck_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_truck_location_history_timestamp",
                table: "truck_location_history",
                column: "timestamp")
                .Annotation("Npgsql:IndexMethod", "brin");

            migrationBuilder.CreateIndex(
                name: "ix_truck_location_history_truck_id_timestamp",
                table: "truck_location_history",
                columns: new[] { "truck_id", "timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ifta_quarter_snapshots");

            migrationBuilder.DropTable(
                name: "truck_jurisdiction_mileage");

            migrationBuilder.DropTable(
                name: "truck_location_history");
        }
    }
}
