using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VehicleConditionReportId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VehicleConditionReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    VehicleYear = table.Column<int>(type: "integer", nullable: true),
                    VehicleMake = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VehicleModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VehicleBodyClass = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DamageMarkersJson = table.Column<string>(type: "jsonb", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    InspectorSignature = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    InspectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InspectedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleConditionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleConditionReports_Employees_InspectedById",
                        column: x => x.InspectedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleConditionReports_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VehicleConditionReportId",
                table: "Documents",
                column: "VehicleConditionReportId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleConditionReports_InspectedAt",
                table: "VehicleConditionReports",
                column: "InspectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleConditionReports_InspectedById",
                table: "VehicleConditionReports",
                column: "InspectedById");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleConditionReports_LoadId",
                table: "VehicleConditionReports",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleConditionReports_Vin",
                table: "VehicleConditionReports",
                column: "Vin");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_VehicleConditionReports_VehicleConditionReportId",
                table: "Documents",
                column: "VehicleConditionReportId",
                principalTable: "VehicleConditionReports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_VehicleConditionReports_VehicleConditionReportId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "VehicleConditionReports");

            migrationBuilder.DropIndex(
                name: "IX_Documents_VehicleConditionReportId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "VehicleConditionReportId",
                table: "Documents");
        }
    }
}
