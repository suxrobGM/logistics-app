using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blog_posts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    excerpt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    author_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    featured_image = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contact_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    message = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_submissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_protection_keys",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    friendly_name = table.Column<string>(type: "text", nullable: true),
                    xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_protection_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "demo_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    fleet_size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_demo_requests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ifta_tax_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    quarter = table.Column<int>(type: "integer", nullable: false),
                    rate_per_gallon = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    surcharge_rate_per_gallon = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: true),
                    jurisdiction_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    jurisdiction_region = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ifta_tax_rates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "impersonation_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    target_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    was_successful = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_impersonation_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "impersonation_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    admin_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_impersonation_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processed_webhook_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    event_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processed_webhook_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_license_heartbeats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hostname = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    key_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    licensee = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    tenant_count = table.Column<int>(type: "integer", nullable: false),
                    first_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_license_heartbeats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "signing_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    algorithm = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_signing_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    tier = table.Column<string>(type: "text", nullable: false),
                    stripe_price_id = table.Column<string>(type: "text", nullable: true),
                    stripe_product_id = table.Column<string>(type: "text", nullable: true),
                    stripe_per_truck_price_id = table.Column<string>(type: "text", nullable: true),
                    stripe_ai_overage_price_id = table.Column<string>(type: "text", nullable: true),
                    weekly_ai_budget_usd = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    max_trucks = table.Column<int>(type: "integer", nullable: true),
                    interval = table.Column<string>(type: "text", nullable: false),
                    interval_count = table.Column<int>(type: "integer", nullable: false),
                    per_truck_price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    per_truck_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    price_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    value = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "telegram_login_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    chat_id = table.Column<long>(type: "bigint", nullable: false),
                    chat_type = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_consumed = table.Column<bool>(type: "boolean", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telegram_login_states", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: true),
                    database_name = table.Column<string>(type: "character varying(63)", maxLength: 63, nullable: false),
                    billing_email = table.Column<string>(type: "text", nullable: false),
                    dot_number = table.Column<string>(type: "text", nullable: true),
                    stripe_customer_id = table.Column<string>(type: "text", nullable: true),
                    logo_path = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    mc_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    vat_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    eori_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    company_registration_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    tax_residency_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    presets = table.Column<string[]>(type: "text[]", nullable: false),
                    is_subscription_required = table.Column<bool>(type: "boolean", nullable: false),
                    quota_reset_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    stripe_connected_account_id = table.Column<string>(type: "text", nullable: true),
                    connect_status = table.Column<string>(type: "text", nullable: false),
                    payouts_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    charges_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    company_address_city = table.Column<string>(type: "text", nullable: false),
                    company_address_country = table.Column<string>(type: "text", nullable: false),
                    company_address_line1 = table.Column<string>(type: "text", nullable: false),
                    company_address_line2 = table.Column<string>(type: "text", nullable: true),
                    company_address_state = table.Column<string>(type: "text", nullable: false),
                    company_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    settings_ai_enabled = table.Column<bool>(type: "boolean", nullable: true),
                    settings_block_ai_overage = table.Column<bool>(type: "boolean", nullable: false),
                    settings_currency = table.Column<string>(type: "text", nullable: false),
                    settings_date_format = table.Column<string>(type: "text", nullable: false),
                    settings_default_rate_floor_per_mile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    settings_detention_free_minutes = table.Column<int>(type: "integer", nullable: true),
                    settings_detention_rate_per_hour = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    settings_distance_unit = table.Column<string>(type: "text", nullable: false),
                    settings_language = table.Column<string>(type: "text", nullable: false),
                    settings_min_broker_credit_score = table.Column<int>(type: "integer", nullable: true),
                    settings_region = table.Column<string>(type: "text", nullable: false),
                    settings_temperature_unit = table.Column<string>(type: "text", nullable: false),
                    settings_timezone = table.Column<string>(type: "text", nullable: false),
                    settings_volume_unit = table.Column<string>(type: "text", nullable: false),
                    settings_weight_unit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_role_claims_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_features",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plan_features", x => x.id);
                    table.ForeignKey(
                        name: "fk_plan_features_subscription_plan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inbound_email_routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    thread_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    purpose = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbound_email_routes", x => x.id);
                    table.ForeignKey(
                        name: "fk_inbound_email_routes_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cancel_at_period_end = table.Column<bool>(type: "boolean", nullable: false),
                    stripe_subscription_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "fk_subscriptions_subscription_plan_plan_id",
                        column: x => x.plan_id,
                        principalTable: "subscription_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_subscriptions_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_feature_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature = table.Column<string>(type: "text", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_admin_locked = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_feature_configs", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_feature_configs_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenant_tax_rates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rate_percent = table.Column<decimal>(type: "numeric(7,4)", precision: 7, scale: 4, nullable: false),
                    tax_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    effective_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    effective_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    jurisdiction_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    jurisdiction_region = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenant_tax_rates", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenant_tax_rates_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    anonymized_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "consent_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consent_type = table.Column<string>(type: "text", nullable: false),
                    granted = table.Column<bool>(type: "boolean", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_consent_records_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_deletion_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    scheduled_for = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_deletion_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_deletion_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_export_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    blob_container = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    blob_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    blob_tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_export_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_export_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    tenant_role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    app_role = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    accepted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    personal_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    send_count = table.Column<int>(type: "integer", nullable: false),
                    last_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invitations", x => x.id);
                    table.ForeignKey(
                        name: "fk_invitations_tenant_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_invitations_users_accepted_by_user_id",
                        column: x => x.accepted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_invitations_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_claims_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_user_logins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tenant_accesses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    customer_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_tenant_accesses", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_tenant_accesses_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_tenant_accesses_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_tokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_user_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_category",
                table: "blog_posts",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_is_featured",
                table: "blog_posts",
                column: "is_featured");

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_published_at",
                table: "blog_posts",
                column: "published_at");

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_slug",
                table: "blog_posts",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_status",
                table: "blog_posts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_consent_type_timestamp",
                table: "consent_records",
                columns: new[] { "consent_type", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_user_id",
                table: "consent_records",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_contact_submissions_created_at",
                table: "contact_submissions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_contact_submissions_email",
                table: "contact_submissions",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_contact_submissions_status",
                table: "contact_submissions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_data_deletion_requests_scheduled_for",
                table: "data_deletion_requests",
                column: "scheduled_for");

            migrationBuilder.CreateIndex(
                name: "ix_data_deletion_requests_status",
                table: "data_deletion_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_data_deletion_requests_user_id",
                table: "data_deletion_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_export_requests_expires_at",
                table: "data_export_requests",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_data_export_requests_requested_at",
                table: "data_export_requests",
                column: "requested_at");

            migrationBuilder.CreateIndex(
                name: "ix_data_export_requests_status",
                table: "data_export_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_data_export_requests_user_id",
                table: "data_export_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_demo_requests_created_at",
                table: "demo_requests",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_demo_requests_email",
                table: "demo_requests",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_ifta_tax_rates_year_quarter",
                table: "ifta_tax_rates",
                columns: new[] { "year", "quarter" });

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_audit_logs_admin_user_id",
                table: "impersonation_audit_logs",
                column: "admin_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_audit_logs_target_user_id",
                table: "impersonation_audit_logs",
                column: "target_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_audit_logs_timestamp",
                table: "impersonation_audit_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_audit_logs_was_successful_timestamp",
                table: "impersonation_audit_logs",
                columns: new[] { "was_successful", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_tokens_expires_at",
                table: "impersonation_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_tokens_token_hash",
                table: "impersonation_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inbound_email_routes_tenant_id",
                table: "inbound_email_routes",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_inbound_email_routes_thread_token",
                table: "inbound_email_routes",
                column: "thread_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invitations_accepted_by_user_id",
                table: "invitations",
                column: "accepted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_email_tenant_id_status",
                table: "invitations",
                columns: new[] { "email", "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_invitations_expires_at",
                table: "invitations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_invited_by_user_id",
                table: "invitations",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_tenant_id",
                table: "invitations",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_invitations_token_hash",
                table: "invitations",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_features_plan_id_feature",
                table: "plan_features",
                columns: new[] { "plan_id", "feature" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_processed_webhook_events_provider_event_key",
                table: "processed_webhook_events",
                columns: new[] { "provider", "event_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_processed_webhook_events_received_at",
                table: "processed_webhook_events",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "ix_product_license_heartbeats_instance_id",
                table: "product_license_heartbeats",
                column: "instance_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_license_heartbeats_last_seen_at",
                table: "product_license_heartbeats",
                column: "last_seen_at");

            migrationBuilder.CreateIndex(
                name: "ix_role_claims_role_id",
                table: "role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_signing_keys_created",
                table: "signing_keys",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "ix_subscription_plans_tier",
                table: "subscription_plans",
                column: "tier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_plan_id",
                table: "subscriptions",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_stripe_subscription_id",
                table: "subscriptions",
                column: "stripe_subscription_id",
                unique: true,
                filter: "stripe_subscription_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_tenant_id",
                table: "subscriptions",
                column: "tenant_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_key",
                table: "system_settings",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_telegram_login_states_state",
                table: "telegram_login_states",
                column: "state",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_feature_configs_tenant_id_feature",
                table: "tenant_feature_configs",
                columns: new[] { "tenant_id", "feature" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenant_tax_rates_tenant_id_effective_from",
                table: "tenant_tax_rates",
                columns: new[] { "tenant_id", "effective_from" });

            migrationBuilder.CreateIndex(
                name: "ix_tenants_name",
                table: "tenants",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_claims_user_id",
                table: "user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_logins_user_id",
                table: "user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_tenant_accesses_tenant_id",
                table: "user_tenant_accesses",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_tenant_accesses_user_id_tenant_id",
                table: "user_tenant_accesses",
                columns: new[] { "user_id", "tenant_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "normalized_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_tenant_id",
                table: "users",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "users",
                column: "normalized_user_name",
                unique: true);

            // Not in the EF model: complex-type members can't be indexed. IftaTaxRateDuplicateChecker relies on it.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_ifta_tax_rates_year_quarter_jurisdiction
                    ON ifta_tax_rates (year, quarter, jurisdiction_country_code, jurisdiction_region)
                    NULLS NOT DISTINCT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_posts");

            migrationBuilder.DropTable(
                name: "consent_records");

            migrationBuilder.DropTable(
                name: "contact_submissions");

            migrationBuilder.DropTable(
                name: "data_deletion_requests");

            migrationBuilder.DropTable(
                name: "data_export_requests");

            migrationBuilder.DropTable(
                name: "data_protection_keys");

            migrationBuilder.DropTable(
                name: "demo_requests");

            migrationBuilder.DropTable(
                name: "ifta_tax_rates");

            migrationBuilder.DropTable(
                name: "impersonation_audit_logs");

            migrationBuilder.DropTable(
                name: "impersonation_tokens");

            migrationBuilder.DropTable(
                name: "inbound_email_routes");

            migrationBuilder.DropTable(
                name: "invitations");

            migrationBuilder.DropTable(
                name: "plan_features");

            migrationBuilder.DropTable(
                name: "processed_webhook_events");

            migrationBuilder.DropTable(
                name: "product_license_heartbeats");

            migrationBuilder.DropTable(
                name: "role_claims");

            migrationBuilder.DropTable(
                name: "signing_keys");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "telegram_login_states");

            migrationBuilder.DropTable(
                name: "tenant_feature_configs");

            migrationBuilder.DropTable(
                name: "tenant_tax_rates");

            migrationBuilder.DropTable(
                name: "user_claims");

            migrationBuilder.DropTable(
                name: "user_logins");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "user_tenant_accesses");

            migrationBuilder.DropTable(
                name: "user_tokens");

            migrationBuilder.DropTable(
                name: "subscription_plans");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
