using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "accounting_provider_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    realm_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    company_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    access_token = table.Column<string>(type: "text", nullable: true),
                    refresh_token = table.Column<string>(type: "text", nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    refresh_token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    needs_reconnect = table.Column<bool>(type: "boolean", nullable: false),
                    default_payment_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    default_payment_account_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    default_expense_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    default_item_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    deposit_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    payroll_expense_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    factoring_clearing_account_id = table.Column<string>(type: "text", nullable: true),
                    factoring_fee_account_id = table.Column<string>(type: "text", nullable: true),
                    sync_from_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    mappings = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounting_provider_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agent_conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    turn_started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_message_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_sequence = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent_conversations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ai_dispatch_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    generated_content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    manual_content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    decisions_analyzed = table.Column<int>(type: "integer", nullable: false),
                    model_used = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_decision_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_edited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_edited_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_dispatch_policies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    key_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    key_prefix = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_api_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "broker_credit_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mc_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    credit_score = table.Column<int>(type: "integer", nullable: true),
                    days_to_pay = table.Column<int>(type: "integer", nullable: true),
                    authority_active = table.Column<bool>(type: "boolean", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_broker_credit_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eld_provider_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    api_key = table.Column<string>(type: "text", nullable: false),
                    api_secret = table.Column<string>(type: "text", nullable: true),
                    webhook_secret = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    region = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eld_provider_configurations", x => x.id);
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
                name: "fuel_card_provider_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    api_key = table.Column<string>(type: "text", nullable: false),
                    api_secret = table.Column<string>(type: "text", nullable: true),
                    access_token = table.Column<string>(type: "text", nullable: true),
                    refresh_token = table.Column<string>(type: "text", nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fuel_card_provider_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ifta_quarter_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    quarter = table.Column<int>(type: "integer", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    report_json = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ifta_quarter_snapshots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lane_rate_floors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    origin_state = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    destination_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    destination_state = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    min_rate_per_mile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    min_total_rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    min_total_rate_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lane_rate_floors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "load_board_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    api_key = table.Column<string>(type: "text", nullable: false),
                    api_secret = table.Column<string>(type: "text", nullable: true),
                    access_token = table.Column<string>(type: "text", nullable: true),
                    refresh_token = table.Column<string>(type: "text", nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    market_rates_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qbo_entity_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_id = table.Column<Guid>(type: "uuid", nullable: false),
                    local_entity_type = table.Column<string>(type: "text", nullable: false),
                    qbo_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    qbo_sync_token = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    last_synced_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sync_status = table.Column<string>(type: "text", nullable: false),
                    last_error = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qbo_entity_mappings", x => x.id);
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
                name: "terminals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: false),
                    address_country = table.Column<string>(type: "text", nullable: false),
                    address_line1 = table.Column<string>(type: "text", nullable: false),
                    address_line2 = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: false),
                    address_zip_code = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_terminals", x => x.id);
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
                name: "agent_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    triggered_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    input_tokens_used = table.Column<int>(type: "integer", nullable: false),
                    output_tokens_used = table.Column<int>(type: "integer", nullable: false),
                    cache_read_tokens = table.Column<int>(type: "integer", nullable: false),
                    cache_creation_tokens = table.Column<int>(type: "integer", nullable: false),
                    estimated_cost_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    model_used = table.Column<string>(type: "text", nullable: true),
                    decision_count = table.Column<int>(type: "integer", nullable: false),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_overage = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_agent_sessions_agent_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "agent_conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    tax_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_vat_exempt = table.Column<bool>(type: "boolean", nullable: false),
                    factoring_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    detention_free_minutes = table.Column<int>(type: "integer", nullable: true),
                    detention_rate_per_hour = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: true),
                    address_country = table.Column<string>(type: "text", nullable: true),
                    address_line1 = table.Column<string>(type: "text", nullable: true),
                    address_line2 = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: true),
                    address_zip_code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                    table.ForeignKey(
                        name: "fk_customers_factoring_company_factoring_company_id",
                        column: x => x.factoring_company_id,
                        principalTable: "factoring_companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "notification_reads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_reads", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_reads_notifications_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notifications",
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
                    pay_excludes_fuel_surcharge = table.Column<bool>(type: "boolean", nullable: false),
                    pay_excludes_accessorials = table.Column<bool>(type: "boolean", nullable: false),
                    deduct_fuel_card_charges = table.Column<bool>(type: "boolean", nullable: false),
                    joined_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_token = table.Column<string>(type: "text", nullable: true),
                    stripe_connected_account_id = table.Column<string>(type: "text", nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: true),
                    address_country = table.Column<string>(type: "text", nullable: true),
                    address_line1 = table.Column<string>(type: "text", nullable: true),
                    address_line2 = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: true),
                    address_zip_code = table.Column<string>(type: "text", nullable: true),
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
                name: "containers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    iso_type = table.Column<string>(type: "text", nullable: false),
                    seal_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    booking_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bill_of_lading_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_laden = table.Column<bool>(type: "boolean", nullable: false),
                    gross_weight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    current_terminal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    loaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    steamship_line_scac = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    steamship_line_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_free_day = table.Column<DateOnly>(type: "date", nullable: true),
                    demurrage_rate_per_day = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    empty_return_deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    per_diem_rate_per_day = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    picked_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    empty_return_terminal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customs_hold = table.Column<bool>(type: "boolean", nullable: false),
                    freight_hold = table.Column<bool>(type: "boolean", nullable: false),
                    line_hold = table.Column<bool>(type: "boolean", nullable: false),
                    other_hold = table.Column<bool>(type: "boolean", nullable: false),
                    available_for_pickup_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pickup_appointment_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pickup_appointment_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pickup_appointment_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    return_appointment_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    return_appointment_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    return_appointment_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    chassis_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    chassis_provider = table.Column<string>(type: "text", nullable: true),
                    chassis_rate_per_day = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    street_turned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    street_turn_booking_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_containers", x => x.id);
                    table.ForeignKey(
                        name: "fk_containers_terminal_current_terminal_id",
                        column: x => x.current_terminal_id,
                        principalTable: "terminals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_containers_terminal_empty_return_terminal_id",
                        column: x => x.empty_return_terminal_id,
                        principalTable: "terminals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "agent_decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    reasoning = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    tool_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tool_input = table.Column<string>(type: "text", nullable: true),
                    tool_output = table.Column<string>(type: "text", nullable: true),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    negotiation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent_decisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_agent_decisions_agent_session_session_id",
                        column: x => x.session_id,
                        principalTable: "agent_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "agent_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    content_json = table.Column<string>(type: "text", nullable: false),
                    display_text = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sent_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_agent_messages_agent_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "agent_conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_agent_messages_agent_session_session_id",
                        column: x => x.session_id,
                        principalTable: "agent_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                name: "driver_hos_statuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    provider_configuration_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                        name: "fk_eld_driver_mappings_eld_provider_configuration_provider_con",
                        column: x => x.provider_configuration_id,
                        principalTable: "eld_provider_configurations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    rule_set_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "FMCSA")
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
                name: "telegram_chats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chat_id = table.Column<long>(type: "bigint", nullable: false),
                    chat_type = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notifications_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    connected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telegram_chats", x => x.id);
                    table.ForeignKey(
                        name: "fk_telegram_chats_employees_user_id",
                        column: x => x.user_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trucks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    vehicle_capacity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    make = table.Column<string>(type: "text", nullable: true),
                    model = table.Column<string>(type: "text", nullable: true),
                    year = table.Column<int>(type: "integer", nullable: true),
                    vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    license_plate = table.Column<string>(type: "text", nullable: true),
                    license_plate_state = table.Column<string>(type: "text", nullable: true),
                    is_hazmat_placarded = table.Column<bool>(type: "boolean", nullable: false),
                    main_driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    secondary_driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    current_trailer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    adr_equipment_adr_cert_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    adr_equipment_allowed_classes = table.Column<string>(type: "text", nullable: false),
                    adr_equipment_is_adr_certified = table.Column<bool>(type: "boolean", nullable: false),
                    adr_equipment_orange_plate_number = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
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
                    table.CheckConstraint("ck_trucks_main_driver_not_secondary", "main_driver_id <> secondary_driver_id");
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
                    table.ForeignKey(
                        name: "fk_trucks_trailers_current_trailer_id",
                        column: x => x.current_trailer_id,
                        principalTable: "trailers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "eld_vehicle_mappings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_configuration_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                        name: "fk_eld_vehicle_mappings_eld_provider_configurations_provider_c",
                        column: x => x.provider_configuration_id,
                        principalTable: "eld_provider_configurations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    category = table.Column<string>(type: "text", nullable: true),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vendor_address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    vendor_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    repair_description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    estimated_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    actual_completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    truck_expense_category = table.Column<string>(type: "text", nullable: true),
                    odometer_reading = table.Column<int>(type: "integer", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    quantity_unit = table.Column<string>(type: "text", nullable: true),
                    fuel_card_provider = table.Column<string>(type: "text", nullable: true),
                    external_transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    price_per_unit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    purchase_jurisdiction_country_code = table.Column<string>(type: "text", nullable: true),
                    purchase_jurisdiction_region = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "fuel_cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    external_card_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unit_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fuel_cards", x => x.id);
                    table.ForeignKey(
                        name: "fk_fuel_cards_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "loads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    fuel_surcharge = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    distance = table.Column<double>(type: "double precision", nullable: false),
                    is_in_proximity = table.Column<bool>(type: "boolean", nullable: false),
                    dispatched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    picked_up_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_dispatcher_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    requested_pickup_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    requested_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    hazmat_class = table.Column<string>(type: "text", nullable: true),
                    un_number = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    container_id = table.Column<Guid>(type: "uuid", nullable: true),
                    origin_terminal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destination_terminal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    external_source_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    freight_class = table.Column<string>(type: "text", nullable: true),
                    nmfc_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    pallet_count = table.Column<int>(type: "integer", nullable: true),
                    piece_count = table.Column<int>(type: "integer", nullable: true),
                    weight = table.Column<double>(type: "double precision", nullable: true),
                    dimensions = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_stackable = table.Column<bool>(type: "boolean", nullable: false),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    vehicles = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_loads", x => x.id);
                    table.ForeignKey(
                        name: "fk_loads_containers_container_id",
                        column: x => x.container_id,
                        principalTable: "containers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                        name: "fk_loads_terminal_destination_terminal_id",
                        column: x => x.destination_terminal_id,
                        principalTable: "terminals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_loads_terminal_origin_terminal_id",
                        column: x => x.origin_terminal_id,
                        principalTable: "terminals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_loads_truck_assigned_truck_id",
                        column: x => x.assigned_truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                        name: "fk_maintenance_records_truck_truck_id",
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
                    equipment_type = table.Column<string>(type: "text", nullable: true),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                    driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    secondary_driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trailer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trips", x => x.id);
                    table.ForeignKey(
                        name: "fk_trips_employees_driver_id",
                        column: x => x.driver_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_trips_employees_secondary_driver_id",
                        column: x => x.secondary_driver_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_trips_trailers_trailer_id",
                        column: x => x.trailer_id,
                        principalTable: "trailers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_trips_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id");
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
                    tax_behavior = table.Column<string>(type: "text", nullable: false, defaultValue: "exclusive"),
                    tax_breakdown_json = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_to_email = table.Column<string>(type: "text", nullable: true),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    tax_total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_total_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    factoring_status = table.Column<string>(type: "text", nullable: true, defaultValue: "not_factored"),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    period_start = table.Column<DateOnly>(type: "date", nullable: true),
                    period_end = table.Column<DateOnly>(type: "date", nullable: true),
                    total_distance_driven = table.Column<double>(type: "double precision", nullable: true),
                    total_hours_worked = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    approved_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approval_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                    equipment_type = table.Column<string>(type: "text", nullable: true),
                    commodity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    broker_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    broker_phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    broker_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    broker_mc_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    broker_credit_score = table.Column<int>(type: "integer", nullable: true),
                    broker_days_to_pay = table.Column<int>(type: "integer", nullable: true),
                    broker_credit_checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    booked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                name: "load_condition_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    load_vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                name: "load_exceptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reported_by_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    resolution = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                    access_count = table.Column<int>(type: "integer", nullable: false),
                    last_accessed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                    departed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    detention_alerted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "fuel_card_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    external_transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    quantity_unit = table.Column<string>(type: "text", nullable: true),
                    price_per_unit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    product_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    merchant_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    merchant_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    card_number_masked = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    external_card_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    unit_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    driver_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payroll_invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    raw_payload_json = table.Column<string>(type: "jsonb", nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    purchase_jurisdiction_country_code = table.Column<string>(type: "text", nullable: true),
                    purchase_jurisdiction_region = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fuel_card_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_fuel_card_transactions_expenses_expense_id",
                        column: x => x.expense_id,
                        principalTable: "expenses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_fuel_card_transactions_invoice_payroll_invoice_id",
                        column: x => x.payroll_invoice_id,
                        principalTable: "invoices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_fuel_card_transactions_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tax_rate_percent = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                    stripe_payment_method_id = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    stripe_payment_intent_id = table.Column<string>(type: "text", nullable: true),
                    stripe_transfer_id = table.Column<string>(type: "text", nullable: true),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reference_number = table.Column<string>(type: "text", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    billing_address_city = table.Column<string>(type: "text", nullable: false),
                    billing_address_country = table.Column<string>(type: "text", nullable: false),
                    billing_address_line1 = table.Column<string>(type: "text", nullable: false),
                    billing_address_line2 = table.Column<string>(type: "text", nullable: true),
                    billing_address_state = table.Column<string>(type: "text", nullable: false),
                    billing_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                name: "rate_negotiations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_board_listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    broker_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    broker_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    broker_mc_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    reply_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    floor_rate_per_mile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    floor_source = table.Column<string>(type: "text", nullable: false),
                    round_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    close_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    last_sequence = table.Column<int>(type: "integer", nullable: false),
                    floor_total_rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    floor_total_rate_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    latest_broker_offer_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    latest_broker_offer_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    latest_counter_offer_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    latest_counter_offer_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rate_negotiations", x => x.id);
                    table.ForeignKey(
                        name: "fk_rate_negotiations_agent_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "agent_conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_rate_negotiations_load_board_listings_load_board_listing_id",
                        column: x => x.load_board_listing_id,
                        principalTable: "load_board_listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rate_negotiations_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    uploaded_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    load_condition_report_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recipient_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    recipient_signature = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    capture_latitude = table.Column<double>(type: "double precision", nullable: true),
                    capture_longitude = table.Column<double>(type: "double precision", nullable: true),
                    captured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    trip_stop_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    unable_to_sign_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_documents", x => x.id);
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
                        name: "fk_documents_load_condition_reports_load_condition_report_id",
                        column: x => x.load_condition_report_id,
                        principalTable: "load_condition_reports",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_documents_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "fk_load_accessorials_trip_stop_trip_stop_id",
                        column: x => x.trip_stop_id,
                        principalTable: "trip_stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
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
                    table.ForeignKey(
                        name: "fk_factoring_batch_items_payment_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "negotiation_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    negotiation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    direction = table.Column<string>(type: "text", nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    text_body = table.Column<string>(type: "character varying(32000)", maxLength: 32000, nullable: false),
                    raw_body = table.Column<string>(type: "character varying(65536)", maxLength: 65536, nullable: true),
                    proposed_rate_per_mile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    rfc_message_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    in_reply_to_message_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    agent_decision_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quarantined = table.Column<bool>(type: "boolean", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    proposed_total_rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    proposed_total_rate_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_negotiation_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_negotiation_messages_agent_decisions_agent_decision_id",
                        column: x => x.agent_decision_id,
                        principalTable: "agent_decisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_negotiation_messages_rate_negotiation_negotiation_id",
                        column: x => x.negotiation_id,
                        principalTable: "rate_negotiations",
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
                    severity = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    zone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    damage_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    photo_document_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_condition_defects", x => x.id);
                    table.ForeignKey(
                        name: "fk_condition_defects_document_photo_document_id",
                        column: x => x.photo_document_id,
                        principalTable: "documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_condition_defects_load_condition_report_load_condition_repo",
                        column: x => x.load_condition_report_id,
                        principalTable: "load_condition_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    issued_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expires_at = table.Column<DateOnly>(type: "date", nullable: false),
                    medical_cert_expires_at = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "active"),
                    document_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_reminder_threshold_days = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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
                name: "ix_accessorial_charges_name",
                table: "accessorial_charges",
                column: "name",
                unique: true);

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
                name: "ix_accounting_provider_configurations_provider_type",
                table: "accounting_provider_configurations",
                column: "provider_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agent_conversations_created_by_id",
                table: "agent_conversations",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_agent_decisions_session_id",
                table: "agent_decisions",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_agent_decisions_status",
                table: "agent_decisions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_agent_messages_conversation_id_sequence",
                table: "agent_messages",
                columns: new[] { "conversation_id", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agent_messages_session_id",
                table: "agent_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_agent_sessions_conversation_id",
                table: "agent_sessions",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_agent_sessions_created_at",
                table: "agent_sessions",
                column: "created_at")
                .Annotation("Npgsql:IndexInclude", new[] { "estimated_cost_usd" });

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_key_hash",
                table: "api_keys",
                column: "key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_broker_credit_records_mc_number",
                table: "broker_credit_records",
                column: "mc_number",
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
                name: "ix_condition_defects_load_condition_report_id",
                table: "condition_defects",
                column: "load_condition_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_condition_defects_part_category",
                table: "condition_defects",
                column: "part_category");

            migrationBuilder.CreateIndex(
                name: "ix_condition_defects_photo_document_id",
                table: "condition_defects",
                column: "photo_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_containers_current_terminal_id",
                table: "containers",
                column: "current_terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_containers_empty_return_terminal_id",
                table: "containers",
                column: "empty_return_terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_containers_number",
                table: "containers",
                column: "number",
                unique: true);

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
                column: "user_id",
                unique: true,
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_customers_factoring_company_id",
                table: "customers",
                column: "factoring_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_employee_id",
                table: "documents",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_load_condition_report_id",
                table: "documents",
                column: "load_condition_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_load_id",
                table: "documents",
                column: "load_id");

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
                name: "ix_driver_hos_statuses_employee_id",
                table: "driver_hos_statuses",
                column: "employee_id",
                unique: true);

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
                name: "ix_eld_driver_mappings_provider_configuration_id",
                table: "eld_driver_mappings",
                column: "provider_configuration_id");

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
                name: "ix_eld_vehicle_mappings_provider_configuration_id",
                table: "eld_vehicle_mappings",
                column: "provider_configuration_id");

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
                name: "ix_expenses_fuel_card_provider_external_transaction_id",
                table: "expenses",
                columns: new[] { "fuel_card_provider", "external_transaction_id" },
                unique: true,
                filter: "external_transaction_id IS NOT NULL");

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
                name: "ix_expenses_type",
                table: "expenses",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_factoring_batch_items_batch_id",
                table: "factoring_batch_items",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_factoring_batch_items_invoice_id",
                table: "factoring_batch_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_factoring_batch_items_payment_id",
                table: "factoring_batch_items",
                column: "payment_id");

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
                name: "ix_fuel_card_provider_configurations_provider_type",
                table: "fuel_card_provider_configurations",
                column: "provider_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_expense_id",
                table: "fuel_card_transactions",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_payroll_invoice_id",
                table: "fuel_card_transactions",
                column: "payroll_invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_provider_type_external_transaction_id",
                table: "fuel_card_transactions",
                columns: new[] { "provider_type", "external_transaction_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_status",
                table: "fuel_card_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_truck_id_transaction_date",
                table: "fuel_card_transactions",
                columns: new[] { "truck_id", "transaction_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fuel_cards_provider_type_external_card_id",
                table: "fuel_cards",
                columns: new[] { "provider_type", "external_card_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fuel_cards_truck_id",
                table: "fuel_cards",
                column: "truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_hos_logs_employee_id_log_date",
                table: "hos_logs",
                columns: new[] { "employee_id", "log_date" });

            migrationBuilder.CreateIndex(
                name: "ix_hos_logs_employee_id_provider_type_external_log_id",
                table: "hos_logs",
                columns: new[] { "employee_id", "provider_type", "external_log_id" },
                unique: true,
                filter: "external_log_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_hos_violations_employee_id_violation_date",
                table: "hos_violations",
                columns: new[] { "employee_id", "violation_date" });

            migrationBuilder.CreateIndex(
                name: "ix_hos_violations_provider_type_external_violation_id",
                table: "hos_violations",
                columns: new[] { "provider_type", "external_violation_id" },
                unique: true,
                filter: "external_violation_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_ifta_quarter_snapshots_year_quarter",
                table: "ifta_quarter_snapshots",
                columns: new[] { "year", "quarter" },
                unique: true);

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
                name: "ix_lane_rate_floors_origin_country_origin_state_destination_co",
                table: "lane_rate_floors",
                columns: new[] { "origin_country", "origin_state", "destination_country", "destination_state" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_load_accessorials_load_id",
                table: "load_accessorials",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_load_accessorials_trip_stop_id",
                table: "load_accessorials",
                column: "trip_stop_id",
                unique: true,
                filter: "line_item_type = 'detention'");

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
                name: "ix_load_condition_reports_load_vehicle_id",
                table: "load_condition_reports",
                column: "load_vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_load_condition_reports_vin",
                table: "load_condition_reports",
                column: "vin");

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
                name: "ix_loads_container_id",
                table: "loads",
                column: "container_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_customer_id",
                table: "loads",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_destination_terminal_id",
                table: "loads",
                column: "destination_terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_number",
                table: "loads",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_loads_origin_terminal_id",
                table: "loads",
                column: "origin_terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_source_external_source_id",
                table: "loads",
                columns: new[] { "source", "external_source_id" },
                unique: true,
                filter: "external_source_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_loads_status",
                table: "loads",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_parts_maintenance_record_id",
                table: "maintenance_parts",
                column: "maintenance_record_id");

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
                name: "ix_negotiation_messages_agent_decision_id",
                table: "negotiation_messages",
                column: "agent_decision_id");

            migrationBuilder.CreateIndex(
                name: "ix_negotiation_messages_negotiation_id_sequence",
                table: "negotiation_messages",
                columns: new[] { "negotiation_id", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notification_reads_notification_id_user_id",
                table: "notification_reads",
                columns: new[] { "notification_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_created_date",
                table: "notifications",
                column: "created_date");

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
                name: "ix_payments_invoice_id",
                table: "payments",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_stripe_payment_intent_id",
                table: "payments",
                column: "stripe_payment_intent_id",
                unique: true,
                filter: "stripe_payment_intent_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_payments_stripe_transfer_id",
                table: "payments",
                column: "stripe_transfer_id",
                unique: true,
                filter: "stripe_transfer_id IS NOT NULL");

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
                name: "ix_qbo_entity_mappings_local_entity_type_local_id",
                table: "qbo_entity_mappings",
                columns: new[] { "local_entity_type", "local_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_qbo_entity_mappings_sync_status",
                table: "qbo_entity_mappings",
                column: "sync_status");

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_conversation_id",
                table: "rate_negotiations",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_expires_at",
                table: "rate_negotiations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_load_board_listing_id",
                table: "rate_negotiations",
                column: "load_board_listing_id",
                unique: true,
                filter: "status IN ('awaiting_broker', 'broker_replied')");

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_load_id",
                table: "rate_negotiations",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_reply_token",
                table: "rate_negotiations",
                column: "reply_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_status",
                table: "rate_negotiations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_recurring_deductions_employee_id",
                table: "recurring_deductions",
                column: "employee_id");

            migrationBuilder.CreateIndex(
                name: "ix_telegram_chats_chat_id",
                table: "telegram_chats",
                column: "chat_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_telegram_chats_user_id",
                table: "telegram_chats",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenant_role_claims_role_id",
                table: "tenant_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_terminals_code",
                table: "terminals",
                column: "code",
                unique: true);

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
                name: "ix_trailers_number",
                table: "trailers",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trailers_vin",
                table: "trailers",
                column: "vin",
                unique: true,
                filter: "vin IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_trip_stops_load_id",
                table: "trip_stops",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_stops_trip_id",
                table: "trip_stops",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_driver_id",
                table: "trips",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_number",
                table: "trips",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trips_secondary_driver_id",
                table: "trips",
                column: "secondary_driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_status",
                table: "trips",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_trips_trailer_id",
                table: "trips",
                column: "trailer_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_truck_id",
                table: "trips",
                column: "truck_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_trucks_current_trailer_id",
                table: "trucks",
                column: "current_trailer_id",
                unique: true,
                filter: "current_trailer_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_trucks_main_driver_id",
                table: "trucks",
                column: "main_driver_id",
                unique: true,
                filter: "main_driver_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_trucks_secondary_driver_id",
                table: "trucks",
                column: "secondary_driver_id",
                unique: true,
                filter: "secondary_driver_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_trucks_vin",
                table: "trucks",
                column: "vin",
                unique: true,
                filter: "vin IS NOT NULL");

            // Not in the EF model (expression and complex-type indexes). The mileage index must match
            // TruckLocationRecorder's ON CONFLICT target exactly.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_customers_name_lower ON customers (lower(name));
                CREATE UNIQUE INDEX ux_employees_email_lower ON employees (lower(email));
                CREATE UNIQUE INDEX ux_trucks_number_lower ON trucks (lower(number));
                CREATE UNIQUE INDEX ux_truck_jurisdiction_mileage_truck_date_jurisdiction
                    ON truck_jurisdiction_mileage (truck_id, date, jurisdiction_country_code, jurisdiction_region)
                    NULLS NOT DISTINCT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accessorial_charges");

            migrationBuilder.DropTable(
                name: "accident_third_parties");

            migrationBuilder.DropTable(
                name: "accident_witnesses");

            migrationBuilder.DropTable(
                name: "accounting_provider_configurations");

            migrationBuilder.DropTable(
                name: "agent_messages");

            migrationBuilder.DropTable(
                name: "ai_dispatch_policies");

            migrationBuilder.DropTable(
                name: "api_keys");

            migrationBuilder.DropTable(
                name: "broker_credit_records");

            migrationBuilder.DropTable(
                name: "cash_advances");

            migrationBuilder.DropTable(
                name: "condition_defects");

            migrationBuilder.DropTable(
                name: "conversation_participants");

            migrationBuilder.DropTable(
                name: "customer_users");

            migrationBuilder.DropTable(
                name: "driver_hos_statuses");

            migrationBuilder.DropTable(
                name: "driver_licenses");

            migrationBuilder.DropTable(
                name: "dvir_defects");

            migrationBuilder.DropTable(
                name: "eld_driver_mappings");

            migrationBuilder.DropTable(
                name: "eld_vehicle_mappings");

            migrationBuilder.DropTable(
                name: "factoring_batch_items");

            migrationBuilder.DropTable(
                name: "fuel_card_provider_configurations");

            migrationBuilder.DropTable(
                name: "fuel_card_transactions");

            migrationBuilder.DropTable(
                name: "fuel_cards");

            migrationBuilder.DropTable(
                name: "hos_logs");

            migrationBuilder.DropTable(
                name: "hos_violations");

            migrationBuilder.DropTable(
                name: "ifta_quarter_snapshots");

            migrationBuilder.DropTable(
                name: "invoice_line_items");

            migrationBuilder.DropTable(
                name: "lane_rate_floors");

            migrationBuilder.DropTable(
                name: "load_accessorials");

            migrationBuilder.DropTable(
                name: "load_board_configurations");

            migrationBuilder.DropTable(
                name: "load_exceptions");

            migrationBuilder.DropTable(
                name: "maintenance_parts");

            migrationBuilder.DropTable(
                name: "message_read_receipts");

            migrationBuilder.DropTable(
                name: "negotiation_messages");

            migrationBuilder.DropTable(
                name: "notification_reads");

            migrationBuilder.DropTable(
                name: "payment_links");

            migrationBuilder.DropTable(
                name: "posted_trucks");

            migrationBuilder.DropTable(
                name: "qbo_entity_mappings");

            migrationBuilder.DropTable(
                name: "recurring_deductions");

            migrationBuilder.DropTable(
                name: "telegram_chats");

            migrationBuilder.DropTable(
                name: "tenant_role_claims");

            migrationBuilder.DropTable(
                name: "time_entries");

            migrationBuilder.DropTable(
                name: "tracking_links");

            migrationBuilder.DropTable(
                name: "truck_jurisdiction_mileage");

            migrationBuilder.DropTable(
                name: "truck_location_history");

            migrationBuilder.DropTable(
                name: "accident_reports");

            migrationBuilder.DropTable(
                name: "documents");

            migrationBuilder.DropTable(
                name: "dvir_reports");

            migrationBuilder.DropTable(
                name: "eld_provider_configurations");

            migrationBuilder.DropTable(
                name: "factoring_batches");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "maintenance_records");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "agent_decisions");

            migrationBuilder.DropTable(
                name: "rate_negotiations");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "load_condition_reports");

            migrationBuilder.DropTable(
                name: "trip_stops");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "agent_sessions");

            migrationBuilder.DropTable(
                name: "load_board_listings");

            migrationBuilder.DropTable(
                name: "trips");

            migrationBuilder.DropTable(
                name: "agent_conversations");

            migrationBuilder.DropTable(
                name: "loads");

            migrationBuilder.DropTable(
                name: "containers");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "trucks");

            migrationBuilder.DropTable(
                name: "terminals");

            migrationBuilder.DropTable(
                name: "factoring_companies");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "trailers");

            migrationBuilder.DropTable(
                name: "tenant_roles");
        }
    }
}
