using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0007 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "adr_equipment_adr_cert_expires_at",
                table: "trucks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "adr_equipment_allowed_classes",
                table: "trucks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "adr_equipment_is_adr_certified",
                table: "trucks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "adr_equipment_orange_plate_number",
                table: "trucks",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_hazmat_placarded",
                table: "trucks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "hazmat_class",
                table: "loads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_hazmat",
                table: "loads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "un_number",
                table: "loads",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "driver_licenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_number = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    license_class = table.Column<string>(type: "text", nullable: false),
                    endorsements = table.Column<string>(type: "text", nullable: false),
                    issuing_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    issuing_region = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    issued_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    medical_cert_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "active"),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_reminder_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_reminder_threshold_days = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_driver_licenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_driver_licenses_document_document_id",
                        column: x => x.document_id,
                        principalTable: "documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_driver_licenses_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_driver_licenses_document_id",
                table: "driver_licenses",
                column: "document_id");

            migrationBuilder.CreateIndex(
                name: "ix_driver_licenses_employee_id_license_number_issuing_country",
                table: "driver_licenses",
                columns: new[] { "employee_id", "license_number", "issuing_country" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_driver_licenses_expires_at",
                table: "driver_licenses",
                column: "expires_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "driver_licenses");

            migrationBuilder.DropColumn(
                name: "adr_equipment_adr_cert_expires_at",
                table: "trucks");

            migrationBuilder.DropColumn(
                name: "adr_equipment_allowed_classes",
                table: "trucks");

            migrationBuilder.DropColumn(
                name: "adr_equipment_is_adr_certified",
                table: "trucks");

            migrationBuilder.DropColumn(
                name: "adr_equipment_orange_plate_number",
                table: "trucks");

            migrationBuilder.DropColumn(
                name: "is_hazmat_placarded",
                table: "trucks");

            migrationBuilder.DropColumn(
                name: "hazmat_class",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "is_hazmat",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "un_number",
                table: "loads");
        }
    }
}
