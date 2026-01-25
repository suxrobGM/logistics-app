using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Logistics.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
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
                name: "LoadBoardConfigurations",
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
                    ExternalAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyDotNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompanyMcNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadBoardConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    StripePaymentMethodId = table.Column<string>(type: "text", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerificationStatus = table.Column<int>(type: "integer", nullable: false),
                    BillingAddress_City = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_Country = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    BillingAddress_State = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    BankName = table.Column<string>(type: "text", nullable: true),
                    AccountNumber = table.Column<string>(type: "text", nullable: true),
                    AccountHolderName = table.Column<string>(type: "text", nullable: true),
                    SwiftCode = table.Column<string>(type: "text", nullable: true),
                    CardHolderName = table.Column<string>(type: "text", nullable: true),
                    CardNumber = table.Column<string>(type: "text", nullable: true),
                    Cvc = table.Column<string>(type: "text", nullable: true),
                    ExpMonth = table.Column<int>(type: "integer", nullable: true),
                    ExpYear = table.Column<int>(type: "integer", nullable: true),
                    UsBankAccountPaymentMethod_BankName = table.Column<string>(type: "text", nullable: true),
                    UsBankAccountPaymentMethod_AccountHolderName = table.Column<string>(type: "text", nullable: true),
                    UsBankAccountPaymentMethod_AccountNumber = table.Column<string>(type: "text", nullable: true),
                    RoutingNumber = table.Column<string>(type: "text", nullable: true),
                    AccountHolderType = table.Column<int>(type: "integer", nullable: true),
                    AccountType = table.Column<int>(type: "integer", nullable: true),
                    VerificationUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    NormalizedName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerUsers_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    SalaryType = table.Column<int>(type: "integer", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceToken = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Salary_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Salary_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: false),
                    ClaimValue = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "Trucks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    VehicleCapacity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MainDriverId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecondaryDriverId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentAddress_City = table.Column<string>(type: "text", nullable: false),
                    CurrentAddress_Country = table.Column<string>(type: "text", nullable: false),
                    CurrentAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    CurrentAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    CurrentAddress_State = table.Column<string>(type: "text", nullable: false),
                    CurrentAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    CurrentLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    CurrentLocation_Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trucks_Employees_MainDriverId",
                        column: x => x.MainDriverId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Trucks_Employees_SecondaryDriverId",
                        column: x => x.SecondaryDriverId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    VendorName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ExpenseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceiptBlobPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ApprovedById = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Amount_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendorAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VendorPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RepairDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EstimatedCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true),
                    TruckExpense_TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    TruckExpense_Category = table.Column<string>(type: "text", nullable: true),
                    OdometerReading = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Trucks_TruckExpense_TruckId",
                        column: x => x.TruckExpense_TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Loads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Distance = table.Column<double>(type: "double precision", nullable: false),
                    CanConfirmPickUp = table.Column<bool>(type: "boolean", nullable: false),
                    CanConfirmDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    DispatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PickedUpAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedTruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignedDispatcherId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalSourceProvider = table.Column<int>(type: "integer", nullable: true),
                    ExternalSourceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExternalBrokerReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeliveryCost_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeliveryCost_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DestinationAddress_City = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Country = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    DestinationAddress_State = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    DestinationLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    DestinationLocation_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    OriginAddress_City = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Country = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    OriginAddress_State = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    OriginLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    OriginLocation_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loads_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Loads_Employees_AssignedDispatcherId",
                        column: x => x.AssignedDispatcherId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Loads_Trucks_AssignedTruckId",
                        column: x => x.AssignedTruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PostedTrucks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    ExternalPostId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DestinationRadius = table.Column<int>(type: "integer", nullable: true),
                    AvailableFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvailableTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EquipmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaxWeight = table.Column<int>(type: "integer", nullable: true),
                    MaxLength = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvailableAtAddress_City = table.Column<string>(type: "text", nullable: false),
                    AvailableAtAddress_Country = table.Column<string>(type: "text", nullable: false),
                    AvailableAtAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    AvailableAtAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    AvailableAtAddress_State = table.Column<string>(type: "text", nullable: false),
                    AvailableAtAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    AvailableAtLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    AvailableAtLocation_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    DestinationPreference_City = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_Country = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_Line1 = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_Line2 = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_State = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_ZipCode = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostedTrucks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostedTrucks_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    TotalDistance = table.Column<double>(type: "double precision", nullable: false),
                    DispatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsTenantChat = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StripeInvoiceId = table.Column<string>(type: "text", nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentToEmail = table.Column<string>(type: "text", nullable: true),
                    Total_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Total_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    BillingPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BillingPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoadBoardListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalListingId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    RatePerMile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Distance = table.Column<double>(type: "double precision", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: true),
                    Length = table.Column<int>(type: "integer", nullable: true),
                    PickupDateStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PickupDateEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDateStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDateEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EquipmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Commodity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BrokerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BrokerPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BrokerEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BrokerMcNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BookedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RawJson = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DestinationAddress_City = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Country = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    DestinationAddress_State = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    DestinationLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    DestinationLocation_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    OriginAddress_City = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Country = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    OriginAddress_State = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    OriginLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    OriginLocation_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    TotalRate_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalRate_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadBoardListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadBoardListings_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TrackingLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessCount = table.Column<int>(type: "integer", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackingLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackingLinks_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "TripStops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ArrivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address_City = table.Column<string>(type: "text", nullable: false),
                    Address_Country = table.Column<string>(type: "text", nullable: false),
                    Address_Line1 = table.Column<string>(type: "text", nullable: false),
                    Address_Line2 = table.Column<string>(type: "text", nullable: true),
                    Address_State = table.Column<string>(type: "text", nullable: false),
                    Address_ZipCode = table.Column<string>(type: "text", nullable: false),
                    Location_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Location_Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripStops_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripStops_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsMuted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationParticipants_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Employees_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Amount_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessCount = table.Column<int>(type: "integer", nullable: false),
                    LastAccessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentLinks_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "text", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "text", nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    BillingAddress_City = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_Country = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    BillingAddress_State = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    BlobContainer = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Active"),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: true),
                    VehicleConditionReportId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecipientName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RecipientSignature = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CaptureLatitude = table.Column<double>(type: "double precision", nullable: true),
                    CaptureLongitude = table.Column<double>(type: "double precision", nullable: true),
                    CapturedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TripStopId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_Employees_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Documents_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_TripStops_TripStopId",
                        column: x => x.TripStopId,
                        principalTable: "TripStops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documents_VehicleConditionReports_VehicleConditionReportId",
                        column: x => x.VehicleConditionReportId,
                        principalTable: "VehicleConditionReports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MessageReadReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadById = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReadReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageReadReceipts_Employees_ReadById",
                        column: x => x.ReadById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageReadReceipts_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_ConversationId_EmployeeId",
                table: "ConversationParticipants",
                columns: new[] { "ConversationId", "EmployeeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConversationParticipants_EmployeeId",
                table: "ConversationParticipants",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_LastMessageAt",
                table: "Conversations",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_LoadId",
                table: "Conversations",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerUsers_CustomerId_UserId",
                table: "CustomerUsers",
                columns: new[] { "CustomerId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerUsers_Email",
                table: "CustomerUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerUsers_UserId",
                table: "CustomerUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_EmployeeId",
                table: "Documents",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_LoadId",
                table: "Documents",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TripStopId",
                table: "Documents",
                column: "TripStopId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TruckId",
                table: "Documents",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_UploadedById",
                table: "Documents",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_VehicleConditionReportId",
                table: "Documents",
                column: "VehicleConditionReportId");

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
                name: "IX_Employees_RoleId",
                table: "Employees",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseDate",
                table: "Expenses",
                column: "ExpenseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Number",
                table: "Expenses",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Status",
                table: "Expenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TruckExpense_TruckId",
                table: "Expenses",
                column: "TruckExpense_TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_TruckId",
                table: "Expenses",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Type",
                table: "Expenses",
                column: "Type");

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

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_InvoiceId",
                table: "InvoiceLineItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_EmployeeId",
                table: "Invoices",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_LoadId",
                table: "Invoices",
                column: "LoadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Number",
                table: "Invoices",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardConfigurations_ProviderType",
                table: "LoadBoardConfigurations",
                column: "ProviderType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardListings_ExpiresAt",
                table: "LoadBoardListings",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardListings_ExternalListingId_ProviderType",
                table: "LoadBoardListings",
                columns: new[] { "ExternalListingId", "ProviderType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardListings_LoadId",
                table: "LoadBoardListings",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardListings_Status",
                table: "LoadBoardListings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_AssignedDispatcherId",
                table: "Loads",
                column: "AssignedDispatcherId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_AssignedTruckId",
                table: "Loads",
                column: "AssignedTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_CustomerId",
                table: "Loads",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_Number",
                table: "Loads",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_MessageId_ReadById",
                table: "MessageReadReceipts",
                columns: new[] { "MessageId", "ReadById" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadReceipts_ReadById",
                table: "MessageReadReceipts",
                column: "ReadById");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SentAt",
                table: "Messages",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLinks_ExpiresAt",
                table: "PaymentLinks",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLinks_InvoiceId",
                table: "PaymentLinks",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLinks_Token",
                table: "PaymentLinks",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethods_StripePaymentMethodId",
                table: "PaymentMethods",
                column: "StripePaymentMethodId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PostedTrucks_ExternalPostId",
                table: "PostedTrucks",
                column: "ExternalPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostedTrucks_Status",
                table: "PostedTrucks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PostedTrucks_TruckId_ProviderType",
                table: "PostedTrucks",
                columns: new[] { "TruckId", "ProviderType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingLinks_ExpiresAt",
                table: "TrackingLinks",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingLinks_LoadId",
                table: "TrackingLinks",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackingLinks_Token",
                table: "TrackingLinks",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_Number",
                table: "Trips",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_TruckId",
                table: "Trips",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStops_LoadId",
                table: "TripStops",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_TripStops_TripId",
                table: "TripStops",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_MainDriverId",
                table: "Trucks",
                column: "MainDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_Number",
                table: "Trucks",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trucks_SecondaryDriverId",
                table: "Trucks",
                column: "SecondaryDriverId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConversationParticipants");

            migrationBuilder.DropTable(
                name: "CustomerUsers");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "DriverHosStatuses");

            migrationBuilder.DropTable(
                name: "EldDriverMappings");

            migrationBuilder.DropTable(
                name: "EldProviderConfigurations");

            migrationBuilder.DropTable(
                name: "EldVehicleMappings");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "HosLogs");

            migrationBuilder.DropTable(
                name: "HosViolations");

            migrationBuilder.DropTable(
                name: "InvoiceLineItems");

            migrationBuilder.DropTable(
                name: "LoadBoardConfigurations");

            migrationBuilder.DropTable(
                name: "LoadBoardListings");

            migrationBuilder.DropTable(
                name: "MessageReadReceipts");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PaymentLinks");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PostedTrucks");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "TrackingLinks");

            migrationBuilder.DropTable(
                name: "TripStops");

            migrationBuilder.DropTable(
                name: "VehicleConditionReports");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Loads");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Trucks");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
