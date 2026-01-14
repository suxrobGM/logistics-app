using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverHosStatuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalDriverId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    CurrentDutyStatus = table.Column<int>(type: "integer", nullable: false),
                    StatusChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DrivingMinutesRemaining = table.Column<int>(type: "integer", nullable: false),
                    OnDutyMinutesRemaining = table.Column<int>(type: "integer", nullable: false),
                    CycleMinutesRemaining = table.Column<int>(type: "integer", nullable: false),
                    TimeUntilBreakRequired = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsInViolation = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextMandatoryBreakAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverHosStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverHosStatuses_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EldDriverMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    ExternalDriverId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalDriverName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsSyncEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EldDriverMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EldDriverMappings_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EldProviderConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApiSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AccessToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RefreshToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WebhookSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EldProviderConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EldVehicleMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    ExternalVehicleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExternalVehicleName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsSyncEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EldVehicleMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EldVehicleMappings_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HosLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DutyStatus = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Remark = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExternalLogId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HosLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HosLogs_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HosViolations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ViolationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ViolationType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SeverityLevel = table.Column<int>(type: "integer", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalViolationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HosViolations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HosViolations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverHosStatuses_EmployeeId",
                table: "DriverHosStatuses",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EldDriverMappings_EmployeeId",
                table: "EldDriverMappings",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EldDriverMappings_ProviderType_EmployeeId",
                table: "EldDriverMappings",
                columns: new[] { "ProviderType", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EldDriverMappings_ProviderType_ExternalDriverId",
                table: "EldDriverMappings",
                columns: new[] { "ProviderType", "ExternalDriverId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EldProviderConfigurations_ProviderType",
                table: "EldProviderConfigurations",
                column: "ProviderType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EldVehicleMappings_ProviderType_ExternalVehicleId",
                table: "EldVehicleMappings",
                columns: new[] { "ProviderType", "ExternalVehicleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EldVehicleMappings_ProviderType_TruckId",
                table: "EldVehicleMappings",
                columns: new[] { "ProviderType", "TruckId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EldVehicleMappings_TruckId",
                table: "EldVehicleMappings",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_HosLogs_EmployeeId_LogDate",
                table: "HosLogs",
                columns: new[] { "EmployeeId", "LogDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HosLogs_ExternalLogId",
                table: "HosLogs",
                column: "ExternalLogId");

            migrationBuilder.CreateIndex(
                name: "IX_HosViolations_EmployeeId_ViolationDate",
                table: "HosViolations",
                columns: new[] { "EmployeeId", "ViolationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HosViolations_ExternalViolationId",
                table: "HosViolations",
                column: "ExternalViolationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverHosStatuses");

            migrationBuilder.DropTable(
                name: "EldDriverMappings");

            migrationBuilder.DropTable(
                name: "EldProviderConfigurations");

            migrationBuilder.DropTable(
                name: "EldVehicleMappings");

            migrationBuilder.DropTable(
                name: "HosLogs");

            migrationBuilder.DropTable(
                name: "HosViolations");
        }
    }
}
