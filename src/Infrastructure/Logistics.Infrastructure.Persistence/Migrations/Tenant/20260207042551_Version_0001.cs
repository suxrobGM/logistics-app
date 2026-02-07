using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: true),
                    address_country = table.Column<string>(type: "text", nullable: true),
                    address_line1 = table.Column<string>(type: "text", nullable: true),
                    address_line2 = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: true),
                    address_zip_code = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eld_provider_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    api_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    api_secret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    access_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    refresh_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    webhook_secret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eld_provider_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "load_board_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    api_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    api_secret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    access_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    refresh_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    webhook_secret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    company_dot_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    company_mc_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_load_board_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    stripe_payment_method_id = table.Column<string>(type: "text", nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    verification_status = table.Column<string>(type: "text", nullable: false),
                    billing_address_city = table.Column<string>(type: "text", nullable: false),
                    billing_address_country = table.Column<string>(type: "text", nullable: false),
                    billing_address_line1 = table.Column<string>(type: "text", nullable: false),
                    billing_address_line2 = table.Column<string>(type: "text", nullable: true),
                    billing_address_state = table.Column<string>(type: "text", nullable: false),
                    billing_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    account_number = table.Column<string>(type: "text", nullable: true),
                    account_holder_name = table.Column<string>(type: "text", nullable: true),
                    swift_code = table.Column<string>(type: "text", nullable: true),
                    card_holder_name = table.Column<string>(type: "text", nullable: true),
                    card_number = table.Column<string>(type: "text", nullable: true),
                    cvc = table.Column<string>(type: "text", nullable: true),
                    exp_month = table.Column<int>(type: "integer", nullable: true),
                    exp_year = table.Column<int>(type: "integer", nullable: true),
                    us_bank_account_payment_method_bank_name = table.Column<string>(type: "text", nullable: true),
                    us_bank_account_payment_method_account_holder_name = table.Column<string>(type: "text", nullable: true),
                    us_bank_account_payment_method_account_number = table.Column<string>(type: "text", nullable: true),
                    routing_number = table.Column<string>(type: "text", nullable: true),
                    account_holder_type = table.Column<string>(type: "text", nullable: true),
                    account_type = table.Column<string>(type: "text", nullable: true),
                    verification_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_methods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenant_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    normalized_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customer_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customer_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_customer_users_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    salary_type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    joined_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_token = table.Column<string>(type: "text", nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: true),
                    salary_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    salary_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employees", x => x.id);
                    table.ForeignKey(
                        name: "fk_employees_tenant_role_role_id",
                        column: x => x.role_id,
                        principalTable: "tenant_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tenant_role_claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: false),
                    claim_value = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_role_claims_tenant_role_role_id",
                        column: x => x.role_id,
                        principalTable: "tenant_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "driver_hos_statuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_driver_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    current_duty_status = table.Column<string>(type: "text", nullable: false),
                    status_changed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    driving_minutes_remaining = table.Column<int>(type: "integer", nullable: false),
                    on_duty_minutes_remaining = table.Column<int>(type: "integer", nullable: false),
                    cycle_minutes_remaining = table.Column<int>(type: "integer", nullable: false),
                    time_until_break_required = table.Column<TimeSpan>(type: "interval", nullable: true),
                    is_in_violation = table.Column<bool>(type: "boolean", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    next_mandatory_break_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_driver_hos_statuses", x => x.id);
                    table.ForeignKey(
                        name: "fk_driver_hos_statuses_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eld_driver_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    external_driver_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_driver_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_sync_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eld_driver_mappings", x => x.id);
                    table.ForeignKey(
                        name: "fk_eld_driver_mappings_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hos_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    log_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duty_status = table.Column<string>(type: "text", nullable: false),
                    start_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    remark = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    external_log_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hos_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_hos_logs_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hos_violations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    violation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    violation_type = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    severity_level = table.Column<int>(type: "integer", nullable: false),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_violation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provider_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hos_violations", x => x.id);
                    table.ForeignKey(
                        name: "fk_hos_violations_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trucks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    vehicle_capacity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    make = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    year = table.Column<int>(type: "integer", nullable: true),
                    vin = table.Column<string>(type: "text", nullable: true),
                    license_plate = table.Column<string>(type: "text", nullable: true),
                    license_plate_state = table.Column<string>(type: "text", nullable: true),
                    main_driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    secondary_driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    current_address_city = table.Column<string>(type: "text", nullable: true),
                    current_address_country = table.Column<string>(type: "text", nullable: true),
                    current_address_line1 = table.Column<string>(type: "text", nullable: true),
                    current_address_line2 = table.Column<string>(type: "text", nullable: true),
                    current_address_state = table.Column<string>(type: "text", nullable: true),
                    current_address_zip_code = table.Column<string>(type: "text", nullable: true),
                    current_location_latitude = table.Column<double>(type: "double precision", nullable: true),
                    current_location_longitude = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trucks", x => x.id);
                    table.ForeignKey(
                        name: "fk_trucks_employees_main_driver_id",
                        column: x => x.main_driver_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_trucks_employees_secondary_driver_id",
                        column: x => x.secondary_driver_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "driver_behavior_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    speed_mph = table.Column<double>(type: "double precision", nullable: true),
                    speed_limit_mph = table.Column<double>(type: "double precision", nullable: true),
                    g_force = table.Column<double>(type: "double precision", nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    external_event_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    raw_event_data_json = table.Column<string>(type: "text", nullable: true),
                    is_reviewed = table.Column<bool>(type: "boolean", nullable: false),
                    reviewed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    review_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_dismissed = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_driver_behavior_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_driver_behavior_events_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_driver_behavior_events_employee_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_driver_behavior_events_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "eld_vehicle_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    external_vehicle_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    external_vehicle_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_sync_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eld_vehicle_mappings", x => x.id);
                    table.ForeignKey(
                        name: "fk_eld_vehicle_mappings_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    vendor_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    expense_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    receipt_blob_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    approved_by_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vendor_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    vendor_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    repair_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    estimated_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    actual_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    category = table.Column<string>(type: "text", nullable: true),
                    truck_expense_truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    truck_expense_category = table.Column<string>(type: "text", nullable: true),
                    odometer_reading = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_expenses", x => x.id);
                    table.ForeignKey(
                        name: "fk_expenses_trucks_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_expenses_trucks_truck_id1",
                        column: x => x.truck_expense_truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "loads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    distance = table.Column<double>(type: "double precision", nullable: false),
                    can_confirm_pick_up = table.Column<bool>(type: "boolean", nullable: false),
                    can_confirm_delivery = table.Column<bool>(type: "boolean", nullable: false),
                    dispatched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    picked_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_dispatcher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_source_provider = table.Column<string>(type: "text", nullable: true),
                    external_source_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_broker_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    delivery_cost_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    delivery_cost_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    destination_address_city = table.Column<string>(type: "text", nullable: false),
                    destination_address_country = table.Column<string>(type: "text", nullable: false),
                    destination_address_line1 = table.Column<string>(type: "text", nullable: false),
                    destination_address_line2 = table.Column<string>(type: "text", nullable: true),
                    destination_address_state = table.Column<string>(type: "text", nullable: false),
                    destination_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    destination_location_latitude = table.Column<double>(type: "double precision", nullable: false),
                    destination_location_longitude = table.Column<double>(type: "double precision", nullable: false),
                    origin_address_city = table.Column<string>(type: "text", nullable: false),
                    origin_address_country = table.Column<string>(type: "text", nullable: false),
                    origin_address_line1 = table.Column<string>(type: "text", nullable: false),
                    origin_address_line2 = table.Column<string>(type: "text", nullable: true),
                    origin_address_state = table.Column<string>(type: "text", nullable: false),
                    origin_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    origin_location_latitude = table.Column<double>(type: "double precision", nullable: false),
                    origin_location_longitude = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loads", x => x.id);
                    table.ForeignKey(
                        name: "fk_loads_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_loads_employees_assigned_dispatcher_id",
                        column: x => x.assigned_dispatcher_id,
                        principalTable: "employees",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_loads_truck_assigned_truck_id",
                        column: x => x.assigned_truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_type = table.Column<string>(type: "text", nullable: false),
                    interval_type = table.Column<string>(type: "text", nullable: false),
                    mileage_interval = table.Column<int>(type: "integer", nullable: true),
                    days_interval = table.Column<int>(type: "integer", nullable: true),
                    engine_hours_interval = table.Column<int>(type: "integer", nullable: true),
                    last_service_mileage = table.Column<int>(type: "integer", nullable: true),
                    last_service_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_service_engine_hours = table.Column<int>(type: "integer", nullable: true),
                    next_due_mileage = table.Column<int>(type: "integer", nullable: true),
                    next_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_due_engine_hours = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_schedules", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_schedules_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "posted_trucks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    external_post_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    destination_radius = table.Column<int>(type: "integer", nullable: true),
                    available_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    available_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    equipment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    max_weight = table.Column<int>(type: "integer", nullable: true),
                    max_length = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_refreshed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    available_at_address_city = table.Column<string>(type: "text", nullable: false),
                    available_at_address_country = table.Column<string>(type: "text", nullable: false),
                    available_at_address_line1 = table.Column<string>(type: "text", nullable: false),
                    available_at_address_line2 = table.Column<string>(type: "text", nullable: true),
                    available_at_address_state = table.Column<string>(type: "text", nullable: false),
                    available_at_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    available_at_location_latitude = table.Column<double>(type: "double precision", nullable: false),
                    available_at_location_longitude = table.Column<double>(type: "double precision", nullable: false),
                    destination_preference_city = table.Column<string>(type: "text", nullable: true),
                    destination_preference_country = table.Column<string>(type: "text", nullable: true),
                    destination_preference_line1 = table.Column<string>(type: "text", nullable: true),
                    destination_preference_line2 = table.Column<string>(type: "text", nullable: true),
                    destination_preference_state = table.Column<string>(type: "text", nullable: true),
                    destination_preference_zip_code = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_posted_trucks", x => x.id);
                    table.ForeignKey(
                        name: "fk_posted_trucks_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trips",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    total_distance = table.Column<double>(type: "double precision", nullable: false),
                    dispatched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trips", x => x.id);
                    table.ForeignKey(
                        name: "fk_trips_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_tenant_chat = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_message_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversations", x => x.id);
                    table.ForeignKey(
                        name: "fk_conversations_load_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    stripe_invoice_id = table.Column<string>(type: "text", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_to_email = table.Column<string>(type: "text", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_distance_driven = table.Column<double>(type: "double precision", nullable: true),
                    total_hours_worked = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    approved_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approval_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: true),
                    billing_period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    billing_period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoices_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_invoices_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_invoices_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "load_board_listings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_listing_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    rate_per_mile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    distance = table.Column<double>(type: "double precision", nullable: true),
                    weight = table.Column<int>(type: "integer", nullable: true),
                    length = table.Column<int>(type: "integer", nullable: true),
                    pickup_date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pickup_date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_date_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivery_date_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    equipment_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    commodity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    broker_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    broker_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    broker_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    broker_mc_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    booked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    raw_json = table.Column<string>(type: "text", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    destination_address_city = table.Column<string>(type: "text", nullable: false),
                    destination_address_country = table.Column<string>(type: "text", nullable: false),
                    destination_address_line1 = table.Column<string>(type: "text", nullable: false),
                    destination_address_line2 = table.Column<string>(type: "text", nullable: true),
                    destination_address_state = table.Column<string>(type: "text", nullable: false),
                    destination_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    destination_location_latitude = table.Column<double>(type: "double precision", nullable: false),
                    destination_location_longitude = table.Column<double>(type: "double precision", nullable: false),
                    origin_address_city = table.Column<string>(type: "text", nullable: false),
                    origin_address_country = table.Column<string>(type: "text", nullable: false),
                    origin_address_line1 = table.Column<string>(type: "text", nullable: false),
                    origin_address_line2 = table.Column<string>(type: "text", nullable: true),
                    origin_address_state = table.Column<string>(type: "text", nullable: false),
                    origin_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    origin_location_latitude = table.Column<double>(type: "double precision", nullable: false),
                    origin_location_longitude = table.Column<double>(type: "double precision", nullable: false),
                    total_rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    total_rate_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_load_board_listings", x => x.id);
                    table.ForeignKey(
                        name: "fk_load_board_listings_load_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "load_exceptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reported_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reported_by_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    resolution = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_load_exceptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_load_exceptions_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tracking_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_count = table.Column<int>(type: "integer", nullable: false),
                    last_accessed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tracking_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_tracking_links_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_condition_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    vehicle_year = table.Column<int>(type: "integer", nullable: true),
                    vehicle_make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_body_class = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    damage_markers_json = table.Column<string>(type: "jsonb", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "maintenance_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_schedule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    maintenance_type = table.Column<string>(type: "text", nullable: false),
                    service_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    odometer_reading = table.Column<int>(type: "integer", nullable: false),
                    engine_hours = table.Column<int>(type: "integer", nullable: true),
                    vendor_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    vendor_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    invoice_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    labor_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    parts_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    work_performed = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    performed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_records_employees_performed_by_id",
                        column: x => x.performed_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_maintenance_records_maintenance_schedule_maintenance_schedu",
                        column: x => x.maintenance_schedule_id,
                        principalTable: "maintenance_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_maintenance_records_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accident_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    accident_type = table.Column<string>(type: "text", nullable: false),
                    severity = table.Column<string>(type: "text", nullable: false),
                    accident_date_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    weather_conditions = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    road_conditions = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    any_injuries = table.Column<bool>(type: "boolean", nullable: false),
                    number_of_injuries = table.Column<int>(type: "integer", nullable: true),
                    injury_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    vehicle_damaged = table.Column<bool>(type: "boolean", nullable: false),
                    vehicle_damage_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    estimated_damage_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    vehicle_drivable = table.Column<bool>(type: "boolean", nullable: false),
                    police_report_filed = table.Column<bool>(type: "boolean", nullable: false),
                    police_report_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    police_officer_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    police_officer_badge = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    police_department = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    insurance_notified = table.Column<bool>(type: "boolean", nullable: false),
                    insurance_notified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    insurance_claim_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    driver_statement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    driver_signature = table.Column<string>(type: "text", nullable: true),
                    driver_signed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    review_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accident_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_accident_reports_employee_driver_id",
                        column: x => x.driver_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_accident_reports_employee_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_accident_reports_trip_trip_id",
                        column: x => x.trip_id,
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_accident_reports_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "dvir_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    driver_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    inspection_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    odometer_reading = table.Column<int>(type: "integer", nullable: true),
                    has_defects = table.Column<bool>(type: "boolean", nullable: false),
                    driver_signature = table.Column<string>(type: "text", nullable: true),
                    driver_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    reviewed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mechanic_signature = table.Column<string>(type: "text", nullable: true),
                    mechanic_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    defects_corrected = table.Column<bool>(type: "boolean", nullable: true),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dvir_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_dvir_reports_employee_driver_id",
                        column: x => x.driver_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_dvir_reports_employee_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_dvir_reports_trip_trip_id",
                        column: x => x.trip_id,
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_dvir_reports_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_stops",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    arrived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_city = table.Column<string>(type: "text", nullable: false),
                    address_country = table.Column<string>(type: "text", nullable: false),
                    address_line1 = table.Column<string>(type: "text", nullable: false),
                    address_line2 = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: false),
                    address_zip_code = table.Column<string>(type: "text", nullable: false),
                    location_latitude = table.Column<double>(type: "double precision", nullable: false),
                    location_longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_stops", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_stops_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trip_stops_trips_trip_id",
                        column: x => x.trip_id,
                        principalTable: "trips",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversation_participants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_muted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_conversation_participants", x => x.id);
                    table.ForeignKey(
                        name: "fk_conversation_participants_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_conversation_participants_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_messages_employees_sender_id",
                        column: x => x.sender_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoice_line_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoice_line_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoice_line_items_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_count = table.Column<int>(type: "integer", nullable: false),
                    last_accessed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_links_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    method_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    stripe_payment_intent_id = table.Column<string>(type: "text", nullable: true),
                    reference_number = table.Column<string>(type: "text", nullable: true),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    billing_address_city = table.Column<string>(type: "text", nullable: false),
                    billing_address_country = table.Column<string>(type: "text", nullable: false),
                    billing_address_line1 = table.Column<string>(type: "text", nullable: false),
                    billing_address_line2 = table.Column<string>(type: "text", nullable: true),
                    billing_address_state = table.Column<string>(type: "text", nullable: false),
                    billing_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_payments_invoices_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "time_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    total_hours = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    payroll_invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_time_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_time_entries_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_time_entries_invoice_payroll_invoice_id",
                        column: x => x.payroll_invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_parts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    maintenance_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    part_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    part_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_parts", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_parts_maintenance_record_maintenance_record_id",
                        column: x => x.maintenance_record_id,
                        principalTable: "maintenance_records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accident_third_parties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accident_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    driver_license = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vehicle_make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_year = table.Column<int>(type: "integer", nullable: true),
                    vehicle_license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    vehicle_vin = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    vehicle_color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    insurance_company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    insurance_policy_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    insurance_agent_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accident_third_parties", x => x.id);
                    table.ForeignKey(
                        name: "fk_accident_third_parties_accident_reports_accident_report_id",
                        column: x => x.accident_report_id,
                        principalTable: "accident_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "accident_witnesses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    accident_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    statement = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accident_witnesses", x => x.id);
                    table.ForeignKey(
                        name: "fk_accident_witnesses_accident_reports_accident_report_id",
                        column: x => x.accident_report_id,
                        principalTable: "accident_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dvir_defects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dvir_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    severity = table.Column<string>(type: "text", nullable: false),
                    is_corrected = table.Column<bool>(type: "boolean", nullable: false),
                    correction_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    corrected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    corrected_by_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dvir_defects", x => x.id);
                    table.ForeignKey(
                        name: "fk_dvir_defects_dvir_report_dvir_report_id",
                        column: x => x.dvir_report_id,
                        principalTable: "dvir_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dvir_defects_employee_corrected_by_id",
                        column: x => x.corrected_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_type = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    file_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    blob_path = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    blob_container = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "active"),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    uploaded_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vehicle_condition_report_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recipient_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    recipient_signature = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    capture_latitude = table.Column<double>(type: "double precision", nullable: true),
                    capture_longitude = table.Column<double>(type: "double precision", nullable: true),
                    captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    trip_stop_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    accident_report_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dvir_report_id = table.Column<Guid>(type: "uuid", nullable: true),
                    maintenance_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_documents_accident_reports_accident_report_id",
                        column: x => x.accident_report_id,
                        principalTable: "accident_reports",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_documents_dvir_reports_dvir_report_id",
                        column: x => x.dvir_report_id,
                        principalTable: "dvir_reports",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_documents_employee_uploaded_by_id",
                        column: x => x.uploaded_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_documents_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_documents_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_documents_maintenance_records_maintenance_record_id",
                        column: x => x.maintenance_record_id,
                        principalTable: "maintenance_records",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_documents_trip_stops_trip_stop_id",
                        column: x => x.trip_stop_id,
                        principalTable: "trip_stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_documents_trucks_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_documents_vehicle_condition_reports_vehicle_condition_repor",
                        column: x => x.vehicle_condition_report_id,
                        principalTable: "vehicle_condition_reports",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "message_read_receipts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    read_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_message_read_receipts", x => x.id);
                    table.ForeignKey(
                        name: "fk_message_read_receipts_employees_read_by_id",
                        column: x => x.read_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_message_read_receipts_messages_message_id",
                        column: x => x.message_id,
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accident_reports_driver_id_accident_date_time",
                table: "accident_reports",
                columns: new[] { "driver_id", "accident_date_time" });

            migrationBuilder.CreateIndex(
                name: "ix_accident_reports_reviewed_by_id",
                table: "accident_reports",
                column: "reviewed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_accident_reports_severity",
                table: "accident_reports",
                column: "severity");

            migrationBuilder.CreateIndex(
                name: "ix_accident_reports_status",
                table: "accident_reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_accident_reports_trip_id",
                table: "accident_reports",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "ix_accident_reports_truck_id",
                table: "accident_reports",
                column: "truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_accident_third_parties_accident_report_id",
                table: "accident_third_parties",
                column: "accident_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_accident_witnesses_accident_report_id",
                table: "accident_witnesses",
                column: "accident_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversation_participants_conversation_id_employee_id",
                table: "conversation_participants",
                columns: new[] { "conversation_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_conversation_participants_employee_id",
                table: "conversation_participants",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversations_last_message_at",
                table: "conversations",
                column: "last_message_at");

            migrationBuilder.CreateIndex(
                name: "ix_conversations_load_id",
                table: "conversations",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_users_customer_id_user_id",
                table: "customer_users",
                columns: new[] { "customer_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customer_users_email",
                table: "customer_users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_customer_users_user_id",
                table: "customer_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_accident_report_id",
                table: "documents",
                column: "accident_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_dvir_report_id",
                table: "documents",
                column: "dvir_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_employee_id",
                table: "documents",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_load_id",
                table: "documents",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_maintenance_record_id",
                table: "documents",
                column: "maintenance_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_trip_stop_id",
                table: "documents",
                column: "trip_stop_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_truck_id",
                table: "documents",
                column: "truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_uploaded_by_id",
                table: "documents",
                column: "uploaded_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_vehicle_condition_report_id",
                table: "documents",
                column: "vehicle_condition_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_employee_id_occurred_at",
                table: "driver_behavior_events",
                columns: new[] { "employee_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_event_type",
                table: "driver_behavior_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_external_event_id",
                table: "driver_behavior_events",
                column: "external_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_reviewed_by_id",
                table: "driver_behavior_events",
                column: "reviewed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_truck_id",
                table: "driver_behavior_events",
                column: "truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_driver_hos_statuses_employee_id",
                table: "driver_hos_statuses",
                column: "employee_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dvir_defects_category_severity",
                table: "dvir_defects",
                columns: new[] { "category", "severity" });

            migrationBuilder.CreateIndex(
                name: "ix_dvir_defects_corrected_by_id",
                table: "dvir_defects",
                column: "corrected_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_dvir_defects_dvir_report_id",
                table: "dvir_defects",
                column: "dvir_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_dvir_reports_driver_id_inspection_date",
                table: "dvir_reports",
                columns: new[] { "driver_id", "inspection_date" });

            migrationBuilder.CreateIndex(
                name: "ix_dvir_reports_reviewed_by_id",
                table: "dvir_reports",
                column: "reviewed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_dvir_reports_status",
                table: "dvir_reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_dvir_reports_trip_id",
                table: "dvir_reports",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "ix_dvir_reports_truck_id_inspection_date",
                table: "dvir_reports",
                columns: new[] { "truck_id", "inspection_date" });

            migrationBuilder.CreateIndex(
                name: "ix_eld_driver_mappings_employee_id",
                table: "eld_driver_mappings",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_eld_driver_mappings_provider_type_employee_id",
                table: "eld_driver_mappings",
                columns: new[] { "provider_type", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_eld_driver_mappings_provider_type_external_driver_id",
                table: "eld_driver_mappings",
                columns: new[] { "provider_type", "external_driver_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_eld_provider_configurations_provider_type",
                table: "eld_provider_configurations",
                column: "provider_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_eld_vehicle_mappings_provider_type_external_vehicle_id",
                table: "eld_vehicle_mappings",
                columns: new[] { "provider_type", "external_vehicle_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_eld_vehicle_mappings_provider_type_truck_id",
                table: "eld_vehicle_mappings",
                columns: new[] { "provider_type", "truck_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_eld_vehicle_mappings_truck_id",
                table: "eld_vehicle_mappings",
                column: "truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_employees_role_id",
                table: "employees",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_expense_date",
                table: "expenses",
                column: "expense_date");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_number",
                table: "expenses",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_expenses_status",
                table: "expenses",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_truck_id",
                table: "expenses",
                column: "truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_truck_id1",
                table: "expenses",
                column: "truck_expense_truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_type",
                table: "expenses",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_hos_logs_employee_id_log_date",
                table: "hos_logs",
                columns: new[] { "employee_id", "log_date" });

            migrationBuilder.CreateIndex(
                name: "ix_hos_logs_external_log_id",
                table: "hos_logs",
                column: "external_log_id");

            migrationBuilder.CreateIndex(
                name: "ix_hos_violations_employee_id_violation_date",
                table: "hos_violations",
                columns: new[] { "employee_id", "violation_date" });

            migrationBuilder.CreateIndex(
                name: "ix_hos_violations_external_violation_id",
                table: "hos_violations",
                column: "external_violation_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoice_line_items_invoice_id",
                table: "invoice_line_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_customer_id",
                table: "invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_employee_id",
                table: "invoices",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_load_id",
                table: "invoices",
                column: "load_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_number",
                table: "invoices",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_load_board_configurations_provider_type",
                table: "load_board_configurations",
                column: "provider_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_load_board_listings_expires_at",
                table: "load_board_listings",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_load_board_listings_external_listing_id_provider_type",
                table: "load_board_listings",
                columns: new[] { "external_listing_id", "provider_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_load_board_listings_load_id",
                table: "load_board_listings",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_load_board_listings_status",
                table: "load_board_listings",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_load_exceptions_load_id",
                table: "load_exceptions",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_load_exceptions_resolved_at",
                table: "load_exceptions",
                column: "resolved_at");

            migrationBuilder.CreateIndex(
                name: "ix_loads_assigned_dispatcher_id",
                table: "loads",
                column: "assigned_dispatcher_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_assigned_truck_id",
                table: "loads",
                column: "assigned_truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_customer_id",
                table: "loads",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_number",
                table: "loads",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_parts_maintenance_record_id",
                table: "maintenance_parts",
                column: "maintenance_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_maintenance_schedule_id",
                table: "maintenance_records",
                column: "maintenance_schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_maintenance_type",
                table: "maintenance_records",
                column: "maintenance_type");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_performed_by_id",
                table: "maintenance_records",
                column: "performed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_truck_id_service_date",
                table: "maintenance_records",
                columns: new[] { "truck_id", "service_date" });

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_is_active",
                table: "maintenance_schedules",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_next_due_date",
                table: "maintenance_schedules",
                column: "next_due_date");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_truck_id_maintenance_type",
                table: "maintenance_schedules",
                columns: new[] { "truck_id", "maintenance_type" });

            migrationBuilder.CreateIndex(
                name: "ix_message_read_receipts_message_id_read_by_id",
                table: "message_read_receipts",
                columns: new[] { "message_id", "read_by_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_message_read_receipts_read_by_id",
                table: "message_read_receipts",
                column: "read_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_conversation_id",
                table: "messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_sender_id",
                table: "messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "ix_messages_sent_at",
                table: "messages",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "ix_payment_links_expires_at",
                table: "payment_links",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_payment_links_invoice_id",
                table: "payment_links",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_links_token",
                table: "payment_links",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_stripe_payment_method_id",
                table: "payment_methods",
                column: "stripe_payment_method_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_invoice_id",
                table: "payments",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_posted_trucks_external_post_id",
                table: "posted_trucks",
                column: "external_post_id");

            migrationBuilder.CreateIndex(
                name: "ix_posted_trucks_status",
                table: "posted_trucks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_posted_trucks_truck_id_provider_type",
                table: "posted_trucks",
                columns: new[] { "truck_id", "provider_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_role_claims_role_id",
                table: "tenant_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_time_entries_employee_id_date",
                table: "time_entries",
                columns: new[] { "employee_id", "date" });

            migrationBuilder.CreateIndex(
                name: "ix_time_entries_payroll_invoice_id",
                table: "time_entries",
                column: "payroll_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_tracking_links_expires_at",
                table: "tracking_links",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_tracking_links_load_id",
                table: "tracking_links",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_tracking_links_token",
                table: "tracking_links",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_stops_load_id",
                table: "trip_stops",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_stops_trip_id",
                table: "trip_stops",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_number",
                table: "trips",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trips_truck_id",
                table: "trips",
                column: "truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_trucks_main_driver_id",
                table: "trucks",
                column: "main_driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_trucks_number",
                table: "trucks",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trucks_secondary_driver_id",
                table: "trucks",
                column: "secondary_driver_id");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accident_third_parties");

            migrationBuilder.DropTable(
                name: "accident_witnesses");

            migrationBuilder.DropTable(
                name: "conversation_participants");

            migrationBuilder.DropTable(
                name: "customer_users");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "driver_behavior_events");

            migrationBuilder.DropTable(
                name: "driver_hos_statuses");

            migrationBuilder.DropTable(
                name: "dvir_defects");

            migrationBuilder.DropTable(
                name: "eld_driver_mappings");

            migrationBuilder.DropTable(
                name: "eld_provider_configurations");

            migrationBuilder.DropTable(
                name: "eld_vehicle_mappings");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "hos_logs");

            migrationBuilder.DropTable(
                name: "hos_violations");

            migrationBuilder.DropTable(
                name: "invoice_line_items");

            migrationBuilder.DropTable(
                name: "load_board_configurations");

            migrationBuilder.DropTable(
                name: "load_board_listings");

            migrationBuilder.DropTable(
                name: "load_exceptions");

            migrationBuilder.DropTable(
                name: "maintenance_parts");

            migrationBuilder.DropTable(
                name: "message_read_receipts");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "payment_links");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "posted_trucks");

            migrationBuilder.DropTable(
                name: "tenant_role_claims");

            migrationBuilder.DropTable(
                name: "time_entries");

            migrationBuilder.DropTable(
                name: "tracking_links");

            migrationBuilder.DropTable(
                name: "accident_reports");

            migrationBuilder.DropTable(
                name: "trip_stops");

            migrationBuilder.DropTable(
                name: "vehicle_condition_reports");

            migrationBuilder.DropTable(
                name: "dvir_reports");

            migrationBuilder.DropTable(
                name: "maintenance_records");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "trips");

            migrationBuilder.DropTable(
                name: "maintenance_schedules");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "loads");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "trucks");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "tenant_roles");
        }
    }
}
