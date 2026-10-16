using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddPilotCarrierFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "webhook_secret",
                table: "load_board_configurations");

            migrationBuilder.DropColumn(
                name: "access_token",
                table: "eld_provider_configurations");

            migrationBuilder.DropColumn(
                name: "refresh_token",
                table: "eld_provider_configurations");

            migrationBuilder.DropColumn(
                name: "token_expires_at",
                table: "eld_provider_configurations");

            migrationBuilder.AddColumn<Guid>(
                name: "current_trailer_id",
                table: "trucks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "trailer_id",
                table: "trips",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "detention_alerted_at",
                table: "trip_stops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "equipment_type",
                table: "posted_trucks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dimensions",
                table: "loads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "freight_class",
                table: "loads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "fuel_surcharge",
                table: "loads",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "is_stackable",
                table: "loads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "nmfc_code",
                table: "loads",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "pallet_count",
                table: "loads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "piece_count",
                table: "loads",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vehicles",
                table: "loads",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<double>(
                name: "weight",
                table: "loads",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "load_vehicle_id",
                table: "load_condition_reports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "equipment_type",
                table: "load_board_listings",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "market_rates_enabled",
                table: "load_board_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "user_email",
                table: "load_board_configurations",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "factoring_status",
                table: "invoices",
                type: "text",
                nullable: true,
                defaultValue: "not_factored");

            migrationBuilder.AddColumn<Guid>(
                name: "source_id",
                table: "invoice_line_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "payroll_invoice_id",
                table: "fuel_card_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "deduct_fuel_card_charges",
                table: "employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "pay_excludes_accessorials",
                table: "employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "pay_excludes_fuel_surcharge",
                table: "employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "provider_configuration_id",
                table: "eld_vehicle_mappings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "region",
                table: "eld_provider_configurations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "provider_configuration_id",
                table: "eld_driver_mappings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "unable_to_sign_reason",
                table: "documents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "detention_free_minutes",
                table: "customers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "detention_rate_per_hour",
                table: "customers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "factoring_company_id",
                table: "customers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "available_for_pickup_at",
                table: "containers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chassis_number",
                table: "containers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "chassis_provider",
                table: "containers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "chassis_rate_per_day",
                table: "containers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "customs_hold",
                table: "containers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "demurrage_rate_per_day",
                table: "containers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "empty_return_deadline",
                table: "containers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "empty_return_terminal_id",
                table: "containers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "freight_hold",
                table: "containers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "last_free_day",
                table: "containers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "line_hold",
                table: "containers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "other_hold",
                table: "containers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "per_diem_rate_per_day",
                table: "containers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "picked_up_at",
                table: "containers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "pickup_appointment_from",
                table: "containers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pickup_appointment_number",
                table: "containers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "pickup_appointment_to",
                table: "containers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "return_appointment_from",
                table: "containers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "return_appointment_number",
                table: "containers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "return_appointment_to",
                table: "containers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "steamship_line_name",
                table: "containers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "steamship_line_scac",
                table: "containers",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "street_turn_booking_reference",
                table: "containers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "street_turned_at",
                table: "containers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "damage_type",
                table: "condition_defects",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "photo_document_id",
                table: "condition_defects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "zone",
                table: "condition_defects",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "default_item_id",
                table: "accounting_provider_configurations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "default_payment_account_type",
                table: "accounting_provider_configurations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deposit_account_id",
                table: "accounting_provider_configurations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "factoring_clearing_account_id",
                table: "accounting_provider_configurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "factoring_fee_account_id",
                table: "accounting_provider_configurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mappings",
                table: "accounting_provider_configurations",
                type: "jsonb",
                nullable: false,
                defaultValue: """{"Items":[],"CompanyExpenseAccounts":[],"TruckExpenseAccounts":[]}""");

            migrationBuilder.AddColumn<bool>(
                name: "needs_reconnect",
                table: "accounting_provider_configurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "payroll_expense_account_id",
                table: "accounting_provider_configurations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "sync_from_date",
                table: "accounting_provider_configurations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "accessorial_charges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    line_item_type = table.Column<string>(type: "text", nullable: false),
                    unit = table.Column<string>(type: "text", nullable: false),
                    default_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accessorial_charges", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cash_advances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    payroll_invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cash_advances", x => x.id);
                    table.ForeignKey(
                        name: "fk_cash_advances_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cash_advances_invoice_payroll_invoice_id",
                        column: x => x.payroll_invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "factoring_companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    submission_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    advance_rate_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    fee_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    remit_to_address_city = table.Column<string>(type: "text", nullable: true),
                    remit_to_address_country = table.Column<string>(type: "text", nullable: true),
                    remit_to_address_line1 = table.Column<string>(type: "text", nullable: true),
                    remit_to_address_line2 = table.Column<string>(type: "text", nullable: true),
                    remit_to_address_state = table.Column<string>(type: "text", nullable: true),
                    remit_to_address_zip_code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_factoring_companies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "load_accessorials",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accessorial_charge_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trip_stop_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    line_item_type = table.Column<string>(type: "text", nullable: false),
                    unit = table.Column<string>(type: "text", nullable: false),
                    rate = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_load_accessorials", x => x.id);
                    table.ForeignKey(
                        name: "fk_load_accessorials_load_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recurring_deductions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    escrow_target = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_recurring_deductions", x => x.id);
                    table.ForeignKey(
                        name: "fk_recurring_deductions_employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trailers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    license_plate_state = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    year = table.Column<int>(type: "integer", nullable: true),
                    make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    length_meters = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trailers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "factoring_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    factoring_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    submitted_to_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_factoring_batches", x => x.id);
                    table.ForeignKey(
                        name: "fk_factoring_batches_factoring_company_factoring_company_id",
                        column: x => x.factoring_company_id,
                        principalTable: "factoring_companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "factoring_batch_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    advance_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    fee_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    reserve_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    reserve_released_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    funded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reserve_released_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    charged_back_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_factoring_batch_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_factoring_batch_items_factoring_batches_batch_id",
                        column: x => x.batch_id,
                        principalTable: "factoring_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_factoring_batch_items_invoice_invoice_id",
                        column: x => x.invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trucks_current_trailer_id",
                table: "trucks",
                column: "current_trailer_id",
                unique: true,
                filter: "current_trailer_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_trips_trailer_id",
                table: "trips",
                column: "trailer_id");

            migrationBuilder.CreateIndex(
                name: "ix_load_condition_reports_load_vehicle_id",
                table: "load_condition_reports",
                column: "load_vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_payroll_invoice_id",
                table: "fuel_card_transactions",
                column: "payroll_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_eld_vehicle_mappings_provider_configuration_id",
                table: "eld_vehicle_mappings",
                column: "provider_configuration_id");

            // ELD mappings predate the foreign keys: point each at its provider's configuration and
            // drop the ones whose provider is no longer configured, or the constraints below fail.
            foreach (var table in new[] { "eld_driver_mappings", "eld_vehicle_mappings" })
            {
                migrationBuilder.Sql($"""
                    UPDATE {table} m SET provider_configuration_id = c.id
                    FROM eld_provider_configurations c
                    WHERE c.provider_type = m.provider_type;

                    DELETE FROM {table}
                    WHERE provider_configuration_id = '00000000-0000-0000-0000-000000000000';
                    """);
            }

            migrationBuilder.CreateIndex(
                name: "ix_eld_driver_mappings_provider_configuration_id",
                table: "eld_driver_mappings",
                column: "provider_configuration_id");

            migrationBuilder.CreateIndex(
                name: "ix_customers_factoring_company_id",
                table: "customers",
                column: "factoring_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_containers_empty_return_terminal_id",
                table: "containers",
                column: "empty_return_terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_accessorial_charges_name",
                table: "accessorial_charges",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cash_advances_employee_id",
                table: "cash_advances",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_cash_advances_payroll_invoice_id",
                table: "cash_advances",
                column: "payroll_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_factoring_batch_items_batch_id",
                table: "factoring_batch_items",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_factoring_batch_items_invoice_id",
                table: "factoring_batch_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_factoring_batches_factoring_company_id",
                table: "factoring_batches",
                column: "factoring_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_factoring_batches_number",
                table: "factoring_batches",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_factoring_batches_status",
                table: "factoring_batches",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_load_accessorials_load_id",
                table: "load_accessorials",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_deductions_employee_id",
                table: "recurring_deductions",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_trailers_number",
                table: "trailers",
                column: "number",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_containers_terminal_empty_return_terminal_id",
                table: "containers",
                column: "empty_return_terminal_id",
                principalTable: "terminals",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_customers_factoring_company_factoring_company_id",
                table: "customers",
                column: "factoring_company_id",
                principalTable: "factoring_companies",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_eld_driver_mappings_eld_provider_configuration_provider_con",
                table: "eld_driver_mappings",
                column: "provider_configuration_id",
                principalTable: "eld_provider_configurations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_eld_vehicle_mappings_eld_provider_configurations_provider_c",
                table: "eld_vehicle_mappings",
                column: "provider_configuration_id",
                principalTable: "eld_provider_configurations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_fuel_card_transactions_invoice_payroll_invoice_id",
                table: "fuel_card_transactions",
                column: "payroll_invoice_id",
                principalTable: "invoices",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_trips_trailers_trailer_id",
                table: "trips",
                column: "trailer_id",
                principalTable: "trailers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_trucks_trailers_current_trailer_id",
                table: "trucks",
                column: "current_trailer_id",
                principalTable: "trailers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // Equipment used to be free text (UI labels such as "Dry Van", or TruckType names for
            // posted trucks). Map what has a LoadBoardEquipmentType counterpart; the rest cannot parse.
            foreach (var table in new[] { "load_board_listings", "posted_trucks" })
            {
                migrationBuilder.Sql($"""
                    UPDATE {table} SET equipment_type =
                        CASE regexp_replace(lower(equipment_type), '[^a-z]', '', 'g')
                            WHEN 'dryvan' THEN 'van'
                            WHEN 'van' THEN 'van'
                            WHEN 'freighttruck' THEN 'van'
                            WHEN 'flatbed' THEN 'flatbed'
                            WHEN 'reefer' THEN 'reefer'
                            WHEN 'stepdeck' THEN 'step_deck'
                            WHEN 'lowboy' THEN 'lowboy'
                            WHEN 'lowloader' THEN 'lowboy'
                            WHEN 'carhauler' THEN 'auto_carrier'
                            WHEN 'cartransporter' THEN 'auto_carrier'
                            WHEN 'autocarrier' THEN 'auto_carrier'
                            WHEN 'boxtruck' THEN 'box_truck'
                            WHEN 'tanker' THEN 'tanker'
                            WHEN 'container' THEN 'container'
                            WHEN 'containertruck' THEN 'container'
                            WHEN 'conestoga' THEN 'conestoga'
                            WHEN 'tautliner' THEN 'conestoga'
                            WHEN 'curtainsider' THEN 'conestoga'
                            WHEN 'hotshot' THEN 'hotshot'
                            WHEN 'poweronly' THEN 'power_only'
                            WHEN 'dumptruck' THEN 'dump_trailer'
                            ELSE NULL
                        END
                    WHERE equipment_type IS NOT NULL;
                    """);
            }

            // Inspection photos used to be matched to their report by load and a capture time within
            // five minutes of the inspection. Link existing photos to the nearest such report so old
            // reports keep their photos now that the query reads load_condition_report_id.
            migrationBuilder.Sql("""
                UPDATE documents d SET load_condition_report_id = m.report_id
                FROM (
                    SELECT DISTINCT ON (doc.id) doc.id AS document_id, r.id AS report_id
                    FROM documents doc
                    JOIN load_condition_reports r ON r.load_id = doc.load_id
                    WHERE doc.load_condition_report_id IS NULL
                      AND doc.type IN ('pickup_inspection', 'delivery_inspection')
                      AND doc.captured_at BETWEEN r.inspected_at - interval '5 minutes'
                                              AND r.inspected_at + interval '5 minutes'
                    ORDER BY doc.id, abs(extract(epoch FROM doc.captured_at - r.inspected_at))
                ) m
                WHERE d.id = m.document_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_containers_terminal_empty_return_terminal_id",
                table: "containers");

            migrationBuilder.DropForeignKey(
                name: "fk_customers_factoring_company_factoring_company_id",
                table: "customers");

            migrationBuilder.DropForeignKey(
                name: "fk_eld_driver_mappings_eld_provider_configuration_provider_con",
                table: "eld_driver_mappings");

            migrationBuilder.DropForeignKey(
                name: "fk_eld_vehicle_mappings_eld_provider_configurations_provider_c",
                table: "eld_vehicle_mappings");

            migrationBuilder.DropForeignKey(
                name: "fk_fuel_card_transactions_invoice_payroll_invoice_id",
                table: "fuel_card_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_trips_trailers_trailer_id",
                table: "trips");

            migrationBuilder.DropForeignKey(
                name: "fk_trucks_trailers_current_trailer_id",
                table: "trucks");

            migrationBuilder.DropTable(
                name: "accessorial_charges");

            migrationBuilder.DropTable(
                name: "cash_advances");

            migrationBuilder.DropTable(
                name: "factoring_batch_items");

            migrationBuilder.DropTable(
                name: "load_accessorials");

            migrationBuilder.DropTable(
                name: "recurring_deductions");

            migrationBuilder.DropTable(
                name: "trailers");

            migrationBuilder.DropTable(
                name: "factoring_batches");

            migrationBuilder.DropTable(
                name: "factoring_companies");

            migrationBuilder.DropIndex(
                name: "ix_trucks_current_trailer_id",
                table: "trucks");

            migrationBuilder.DropIndex(
                name: "ix_trips_trailer_id",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_load_condition_reports_load_vehicle_id",
                table: "load_condition_reports");

            migrationBuilder.DropIndex(
                name: "ix_fuel_card_transactions_payroll_invoice_id",
                table: "fuel_card_transactions");

            migrationBuilder.DropIndex(
                name: "ix_eld_vehicle_mappings_provider_configuration_id",
                table: "eld_vehicle_mappings");

            migrationBuilder.DropIndex(
                name: "ix_eld_driver_mappings_provider_configuration_id",
                table: "eld_driver_mappings");

            migrationBuilder.DropIndex(
                name: "ix_customers_factoring_company_id",
                table: "customers");

            migrationBuilder.DropIndex(
                name: "ix_containers_empty_return_terminal_id",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "current_trailer_id",
                table: "trucks");

            migrationBuilder.DropColumn(
                name: "trailer_id",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "detention_alerted_at",
                table: "trip_stops");

            migrationBuilder.DropColumn(
                name: "dimensions",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "freight_class",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "fuel_surcharge",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "is_stackable",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "nmfc_code",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "pallet_count",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "piece_count",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "vehicles",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "weight",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "load_vehicle_id",
                table: "load_condition_reports");

            migrationBuilder.DropColumn(
                name: "market_rates_enabled",
                table: "load_board_configurations");

            migrationBuilder.DropColumn(
                name: "user_email",
                table: "load_board_configurations");

            migrationBuilder.DropColumn(
                name: "factoring_status",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "source_id",
                table: "invoice_line_items");

            migrationBuilder.DropColumn(
                name: "payroll_invoice_id",
                table: "fuel_card_transactions");

            migrationBuilder.DropColumn(
                name: "deduct_fuel_card_charges",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "pay_excludes_accessorials",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "pay_excludes_fuel_surcharge",
                table: "employees");

            migrationBuilder.DropColumn(
                name: "provider_configuration_id",
                table: "eld_vehicle_mappings");

            migrationBuilder.DropColumn(
                name: "region",
                table: "eld_provider_configurations");

            migrationBuilder.DropColumn(
                name: "provider_configuration_id",
                table: "eld_driver_mappings");

            migrationBuilder.DropColumn(
                name: "unable_to_sign_reason",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "detention_free_minutes",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "detention_rate_per_hour",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "factoring_company_id",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "available_for_pickup_at",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "chassis_number",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "chassis_provider",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "chassis_rate_per_day",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "customs_hold",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "demurrage_rate_per_day",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "empty_return_deadline",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "empty_return_terminal_id",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "freight_hold",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "last_free_day",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "line_hold",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "other_hold",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "per_diem_rate_per_day",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "picked_up_at",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "pickup_appointment_from",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "pickup_appointment_number",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "pickup_appointment_to",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "return_appointment_from",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "return_appointment_number",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "return_appointment_to",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "steamship_line_name",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "steamship_line_scac",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "street_turn_booking_reference",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "street_turned_at",
                table: "containers");

            migrationBuilder.DropColumn(
                name: "damage_type",
                table: "condition_defects");

            migrationBuilder.DropColumn(
                name: "photo_document_id",
                table: "condition_defects");

            migrationBuilder.DropColumn(
                name: "zone",
                table: "condition_defects");

            migrationBuilder.DropColumn(
                name: "default_item_id",
                table: "accounting_provider_configurations");

            migrationBuilder.DropColumn(
                name: "default_payment_account_type",
                table: "accounting_provider_configurations");

            migrationBuilder.DropColumn(
                name: "deposit_account_id",
                table: "accounting_provider_configurations");

            migrationBuilder.DropColumn(
                name: "factoring_clearing_account_id",
                table: "accounting_provider_configurations");

            migrationBuilder.DropColumn(
                name: "factoring_fee_account_id",
                table: "accounting_provider_configurations");

            migrationBuilder.DropColumn(
                name: "mappings",
                table: "accounting_provider_configurations");

            migrationBuilder.DropColumn(
                name: "needs_reconnect",
                table: "accounting_provider_configurations");

            migrationBuilder.DropColumn(
                name: "payroll_expense_account_id",
                table: "accounting_provider_configurations");

            migrationBuilder.DropColumn(
                name: "sync_from_date",
                table: "accounting_provider_configurations");

            migrationBuilder.AlterColumn<string>(
                name: "equipment_type",
                table: "posted_trucks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "equipment_type",
                table: "load_board_listings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "webhook_secret",
                table: "load_board_configurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "access_token",
                table: "eld_provider_configurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "refresh_token",
                table: "eld_provider_configurations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "token_expires_at",
                table: "eld_provider_configurations",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
