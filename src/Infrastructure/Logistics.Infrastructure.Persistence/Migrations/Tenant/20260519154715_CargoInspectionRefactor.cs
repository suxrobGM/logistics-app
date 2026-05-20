using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class CargoInspectionRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_documents_vehicle_condition_reports_vehicle_condition_repor",
                table: "documents");

            migrationBuilder.DropTable(
                name: "vehicle_condition_reports");

            migrationBuilder.RenameColumn(
                name: "vehicle_condition_report_id",
                table: "documents",
                newName: "load_condition_report_id");

            migrationBuilder.RenameIndex(
                name: "ix_documents_vehicle_condition_report_id",
                table: "documents",
                newName: "ix_documents_load_condition_report_id");

            migrationBuilder.CreateTable(
                name: "load_condition_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    vehicle_year = table.Column<int>(type: "integer", nullable: true),
                    vehicle_make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_body_class = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    container_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    seal_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    inspector_signature = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    inspected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    inspected_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_load_condition_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_load_condition_reports_employees_inspected_by_id",
                        column: x => x.inspected_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_load_condition_reports_load_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "condition_defects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_condition_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    part_category = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_condition_defects", x => x.id);
                    table.ForeignKey(
                        name: "fk_condition_defects_load_condition_report_load_condition_repo",
                        column: x => x.load_condition_report_id,
                        principalTable: "load_condition_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_condition_defects_load_condition_report_id",
                table: "condition_defects",
                column: "load_condition_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_condition_defects_part_category",
                table: "condition_defects",
                column: "part_category");

            migrationBuilder.CreateIndex(
                name: "ix_load_condition_reports_container_number",
                table: "load_condition_reports",
                column: "container_number");

            migrationBuilder.CreateIndex(
                name: "ix_load_condition_reports_inspected_at",
                table: "load_condition_reports",
                column: "inspected_at");

            migrationBuilder.CreateIndex(
                name: "ix_load_condition_reports_inspected_by_id",
                table: "load_condition_reports",
                column: "inspected_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_load_condition_reports_load_id",
                table: "load_condition_reports",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_load_condition_reports_vin",
                table: "load_condition_reports",
                column: "vin");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_load_condition_reports_load_condition_report_id",
                table: "documents",
                column: "load_condition_report_id",
                principalTable: "load_condition_reports",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_documents_load_condition_reports_load_condition_report_id",
                table: "documents");

            migrationBuilder.DropTable(
                name: "condition_defects");

            migrationBuilder.DropTable(
                name: "load_condition_reports");

            migrationBuilder.RenameColumn(
                name: "load_condition_report_id",
                table: "documents",
                newName: "vehicle_condition_report_id");

            migrationBuilder.RenameIndex(
                name: "ix_documents_load_condition_report_id",
                table: "documents",
                newName: "ix_documents_vehicle_condition_report_id");

            migrationBuilder.CreateTable(
                name: "vehicle_condition_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    inspected_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    damage_markers_json = table.Column<string>(type: "jsonb", nullable: true),
                    inspected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    inspector_signature = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vehicle_body_class = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_year = table.Column<int>(type: "integer", nullable: true),
                    vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_condition_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_condition_reports_employees_inspected_by_id",
                        column: x => x.inspected_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicle_condition_reports_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_condition_reports_inspected_at",
                table: "vehicle_condition_reports",
                column: "inspected_at");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_condition_reports_inspected_by_id",
                table: "vehicle_condition_reports",
                column: "inspected_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_condition_reports_load_id",
                table: "vehicle_condition_reports",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_condition_reports_vin",
                table: "vehicle_condition_reports",
                column: "vin");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_vehicle_condition_reports_vehicle_condition_repor",
                table: "documents",
                column: "vehicle_condition_report_id",
                principalTable: "vehicle_condition_reports",
                principalColumn: "id");
        }
    }
}
