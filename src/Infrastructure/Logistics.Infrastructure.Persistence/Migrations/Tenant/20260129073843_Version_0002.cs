using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicensePlateState",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AccidentReportId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DriverCertificationId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DvirReportId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MaintenanceRecordId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingRecordId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccidentReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AccidentType = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    AccidentDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    WeatherConditions = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RoadConditions = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AnyInjuries = table.Column<bool>(type: "boolean", nullable: false),
                    NumberOfInjuries = table.Column<int>(type: "integer", nullable: true),
                    InjuryDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    VehicleDamaged = table.Column<bool>(type: "boolean", nullable: false),
                    VehicleDamageDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EstimatedDamageCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    VehicleDrivable = table.Column<bool>(type: "boolean", nullable: false),
                    PoliceReportFiled = table.Column<bool>(type: "boolean", nullable: false),
                    PoliceReportNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PoliceOfficerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PoliceOfficerBadge = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PoliceDepartment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InsuranceNotified = table.Column<bool>(type: "boolean", nullable: false),
                    InsuranceNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InsuranceClaimNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DriverStatement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    DriverSignature = table.Column<string>(type: "text", nullable: true),
                    DriverSignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccidentReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccidentReports_Employees_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccidentReports_Employees_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccidentReports_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AccidentReports_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DriverBehaviorEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SpeedMph = table.Column<double>(type: "double precision", nullable: true),
                    SpeedLimitMph = table.Column<double>(type: "double precision", nullable: true),
                    GForce = table.Column<double>(type: "double precision", nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: true),
                    ExternalEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RawEventDataJson = table.Column<string>(type: "text", nullable: true),
                    IsReviewed = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsDismissed = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverBehaviorEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverBehaviorEvents_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverBehaviorEvents_Employees_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverBehaviorEvents_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DriverCertifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationType = table.Column<int>(type: "integer", nullable: false),
                    CertificationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuingState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CdlClass = table.Column<int>(type: "integer", nullable: true),
                    Endorsements = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Restrictions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverCertifications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverCertifications_Employees_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DvirReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    OdometerReading = table.Column<int>(type: "integer", nullable: true),
                    HasDefects = table.Column<bool>(type: "boolean", nullable: false),
                    DriverSignature = table.Column<string>(type: "text", nullable: true),
                    DriverNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MechanicSignature = table.Column<string>(type: "text", nullable: true),
                    MechanicNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DefectsCorrected = table.Column<bool>(type: "boolean", nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DvirReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DvirReports_Employees_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DvirReports_Employees_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DvirReports_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DvirReports_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DriverId = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    TripId = table.Column<Guid>(type: "uuid", nullable: true),
                    AlertType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    TriggeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AcknowledgedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_Employees_AcknowledgedById",
                        column: x => x.AcknowledgedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_Employees_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_Employees_ResolvedById",
                        column: x => x.ResolvedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EmergencyAlerts_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactType = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyContacts_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceType = table.Column<int>(type: "integer", nullable: false),
                    IntervalType = table.Column<int>(type: "integer", nullable: false),
                    MileageInterval = table.Column<int>(type: "integer", nullable: true),
                    DaysInterval = table.Column<int>(type: "integer", nullable: true),
                    EngineHoursInterval = table.Column<int>(type: "integer", nullable: true),
                    LastServiceMileage = table.Column<int>(type: "integer", nullable: true),
                    LastServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastServiceEngineHours = table.Column<int>(type: "integer", nullable: true),
                    NextDueMileage = table.Column<int>(type: "integer", nullable: true),
                    NextDueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextDueEngineHours = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceSchedules_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrainingType = table.Column<int>(type: "integer", nullable: false),
                    TrainingName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Hours = table.Column<decimal>(type: "numeric", nullable: true),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccidentThirdParties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccidentReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DriverLicense = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VehicleMake = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VehicleModel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    VehicleYear = table.Column<int>(type: "integer", nullable: true),
                    VehicleLicensePlate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VehicleVin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    VehicleColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    InsuranceCompany = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    InsurancePolicyNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InsuranceAgentPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccidentThirdParties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccidentThirdParties_AccidentReports_AccidentReportId",
                        column: x => x.AccidentReportId,
                        principalTable: "AccidentReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccidentWitnesses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccidentReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Statement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccidentWitnesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccidentWitnesses_AccidentReports_AccidentReportId",
                        column: x => x.AccidentReportId,
                        principalTable: "AccidentReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DvirDefects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DvirReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    IsCorrected = table.Column<bool>(type: "boolean", nullable: false),
                    CorrectionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CorrectedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DvirDefects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DvirDefects_DvirReports_DvirReportId",
                        column: x => x.DvirReportId,
                        principalTable: "DvirReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DvirDefects_Employees_CorrectedById",
                        column: x => x.CorrectedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyContactNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyAlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDelivered = table.Column<bool>(type: "boolean", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "boolean", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyContactNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyContactNotifications_EmergencyAlerts_EmergencyAler~",
                        column: x => x.EmergencyAlertId,
                        principalTable: "EmergencyAlerts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmergencyContactNotifications_EmergencyContacts_EmergencyCo~",
                        column: x => x.EmergencyContactId,
                        principalTable: "EmergencyContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaintenanceType = table.Column<int>(type: "integer", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OdometerReading = table.Column<int>(type: "integer", nullable: false),
                    EngineHours = table.Column<int>(type: "integer", nullable: true),
                    VendorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VendorAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LaborCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PartsCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WorkPerformed = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PerformedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_Employees_PerformedById",
                        column: x => x.PerformedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_MaintenanceSchedules_MaintenanceSchedule~",
                        column: x => x.MaintenanceScheduleId,
                        principalTable: "MaintenanceSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaintenanceRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PartNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceParts_MaintenanceRecords_MaintenanceRecordId",
                        column: x => x.MaintenanceRecordId,
                        principalTable: "MaintenanceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_AccidentReportId",
                table: "Documents",
                column: "AccidentReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DriverCertificationId",
                table: "Documents",
                column: "DriverCertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DvirReportId",
                table: "Documents",
                column: "DvirReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_MaintenanceRecordId",
                table: "Documents",
                column: "MaintenanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TrainingRecordId",
                table: "Documents",
                column: "TrainingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentReports_DriverId_AccidentDateTime",
                table: "AccidentReports",
                columns: new[] { "DriverId", "AccidentDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AccidentReports_ReviewedById",
                table: "AccidentReports",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentReports_Severity",
                table: "AccidentReports",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentReports_Status",
                table: "AccidentReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentReports_TripId",
                table: "AccidentReports",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentReports_TruckId",
                table: "AccidentReports",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentThirdParties_AccidentReportId",
                table: "AccidentThirdParties",
                column: "AccidentReportId");

            migrationBuilder.CreateIndex(
                name: "IX_AccidentWitnesses_AccidentReportId",
                table: "AccidentWitnesses",
                column: "AccidentReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverBehaviorEvents_EmployeeId_OccurredAt",
                table: "DriverBehaviorEvents",
                columns: new[] { "EmployeeId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverBehaviorEvents_EventType",
                table: "DriverBehaviorEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_DriverBehaviorEvents_ExternalEventId",
                table: "DriverBehaviorEvents",
                column: "ExternalEventId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverBehaviorEvents_ReviewedById",
                table: "DriverBehaviorEvents",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_DriverBehaviorEvents_TruckId",
                table: "DriverBehaviorEvents",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertifications_EmployeeId_CertificationType",
                table: "DriverCertifications",
                columns: new[] { "EmployeeId", "CertificationType" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertifications_ExpirationDate",
                table: "DriverCertifications",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertifications_Status",
                table: "DriverCertifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertifications_VerifiedById",
                table: "DriverCertifications",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_DvirDefects_Category_Severity",
                table: "DvirDefects",
                columns: new[] { "Category", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_DvirDefects_CorrectedById",
                table: "DvirDefects",
                column: "CorrectedById");

            migrationBuilder.CreateIndex(
                name: "IX_DvirDefects_DvirReportId",
                table: "DvirDefects",
                column: "DvirReportId");

            migrationBuilder.CreateIndex(
                name: "IX_DvirReports_DriverId_InspectionDate",
                table: "DvirReports",
                columns: new[] { "DriverId", "InspectionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DvirReports_ReviewedById",
                table: "DvirReports",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_DvirReports_Status",
                table: "DvirReports",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DvirReports_TripId",
                table: "DvirReports",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_DvirReports_TruckId_InspectionDate",
                table: "DvirReports",
                columns: new[] { "TruckId", "InspectionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_AcknowledgedById",
                table: "EmergencyAlerts",
                column: "AcknowledgedById");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_DriverId",
                table: "EmergencyAlerts",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_ResolvedById",
                table: "EmergencyAlerts",
                column: "ResolvedById");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_Status",
                table: "EmergencyAlerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_TriggeredAt",
                table: "EmergencyAlerts",
                column: "TriggeredAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_TripId",
                table: "EmergencyAlerts",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAlerts_TruckId",
                table: "EmergencyAlerts",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContactNotifications_EmergencyAlertId",
                table: "EmergencyContactNotifications",
                column: "EmergencyAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContactNotifications_EmergencyContactId",
                table: "EmergencyContactNotifications",
                column: "EmergencyContactId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_ContactType",
                table: "EmergencyContacts",
                column: "ContactType");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_EmployeeId",
                table: "EmergencyContacts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyContacts_Priority",
                table: "EmergencyContacts",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceParts_MaintenanceRecordId",
                table: "MaintenanceParts",
                column: "MaintenanceRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_MaintenanceScheduleId",
                table: "MaintenanceRecords",
                column: "MaintenanceScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_MaintenanceType",
                table: "MaintenanceRecords",
                column: "MaintenanceType");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_PerformedById",
                table: "MaintenanceRecords",
                column: "PerformedById");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_TruckId_ServiceDate",
                table: "MaintenanceRecords",
                columns: new[] { "TruckId", "ServiceDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_IsActive",
                table: "MaintenanceSchedules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_NextDueDate",
                table: "MaintenanceSchedules",
                column: "NextDueDate");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceSchedules_TruckId_MaintenanceType",
                table: "MaintenanceSchedules",
                columns: new[] { "TruckId", "MaintenanceType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecords_CompletedDate",
                table: "TrainingRecords",
                column: "CompletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecords_EmployeeId_TrainingType",
                table: "TrainingRecords",
                columns: new[] { "EmployeeId", "TrainingType" });

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AccidentReports_AccidentReportId",
                table: "Documents",
                column: "AccidentReportId",
                principalTable: "AccidentReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_DriverCertifications_DriverCertificationId",
                table: "Documents",
                column: "DriverCertificationId",
                principalTable: "DriverCertifications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_DvirReports_DvirReportId",
                table: "Documents",
                column: "DvirReportId",
                principalTable: "DvirReports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_MaintenanceRecords_MaintenanceRecordId",
                table: "Documents",
                column: "MaintenanceRecordId",
                principalTable: "MaintenanceRecords",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_TrainingRecords_TrainingRecordId",
                table: "Documents",
                column: "TrainingRecordId",
                principalTable: "TrainingRecords",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AccidentReports_AccidentReportId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_DriverCertifications_DriverCertificationId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_DvirReports_DvirReportId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_MaintenanceRecords_MaintenanceRecordId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_TrainingRecords_TrainingRecordId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "AccidentThirdParties");

            migrationBuilder.DropTable(
                name: "AccidentWitnesses");

            migrationBuilder.DropTable(
                name: "DriverBehaviorEvents");

            migrationBuilder.DropTable(
                name: "DriverCertifications");

            migrationBuilder.DropTable(
                name: "DvirDefects");

            migrationBuilder.DropTable(
                name: "EmergencyContactNotifications");

            migrationBuilder.DropTable(
                name: "MaintenanceParts");

            migrationBuilder.DropTable(
                name: "TrainingRecords");

            migrationBuilder.DropTable(
                name: "AccidentReports");

            migrationBuilder.DropTable(
                name: "DvirReports");

            migrationBuilder.DropTable(
                name: "EmergencyAlerts");

            migrationBuilder.DropTable(
                name: "EmergencyContacts");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords");

            migrationBuilder.DropTable(
                name: "MaintenanceSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Documents_AccidentReportId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DriverCertificationId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DvirReportId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_MaintenanceRecordId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_TrainingRecordId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "LicensePlateState",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "AccidentReportId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DriverCertificationId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DvirReportId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "MaintenanceRecordId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "TrainingRecordId",
                table: "Documents");
        }
    }
}
