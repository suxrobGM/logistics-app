using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class PilotDataIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pre-flights: duplicates these unique indexes would reject can't be resolved without picking a
            // winner, so the migration stops and lists them instead.
            migrationBuilder.Sql(FailIfAny(
                "Master invoices/payments are not empty and would be dropped",
                """
                SELECT 'payments: ' || count(*) FROM payments HAVING count(*) > 0
                UNION ALL
                SELECT 'invoices: ' || count(*) FROM invoices HAVING count(*) > 0
                """));

            migrationBuilder.Sql(FailIfAny(
                "Tenant names must be unique and at most 64 characters; rename these by hand",
                """
                SELECT 'duplicate ' || name || ' x' || count(*) FROM tenants GROUP BY name HAVING count(*) > 1
                UNION ALL
                SELECT 'too long ' || id FROM tenants WHERE length(name) > 64
                """));

            migrationBuilder.Sql(FailIfAny(
                "Users share a normalized email; merge the duplicate accounts by hand",
                """
                SELECT normalized_email || ' x' || count(*) FROM users
                WHERE normalized_email IS NOT NULL
                GROUP BY normalized_email HAVING count(*) > 1
                """));

            migrationBuilder.Sql(FailIfAny(
                "Subscriptions share a Stripe subscription id; resolve by hand",
                """
                SELECT stripe_subscription_id || ' x' || count(*) FROM subscriptions
                WHERE stripe_subscription_id IS NOT NULL
                GROUP BY stripe_subscription_id HAVING count(*) > 1
                """));

            migrationBuilder.Sql(FailIfAny(
                "IFTA tax rates repeat a quarter and jurisdiction; delete the wrong rate by hand",
                """
                SELECT year || ' Q' || quarter || ' ' || jurisdiction_country_code
                       || coalesce('-' || jurisdiction_region, '') || ' x' || count(*)
                FROM ifta_tax_rates
                GROUP BY year, quarter, jurisdiction_country_code, jurisdiction_region HAVING count(*) > 1
                """));

            migrationBuilder.DropTable(
                name: "default_feature_configs");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_user_tenant_accesses_user_id",
                table: "user_tenant_accesses");

            migrationBuilder.DropIndex(
                name: "ix_impersonation_tokens_is_used_expires_at",
                table: "impersonation_tokens");

            migrationBuilder.DropColumn(
                name: "deletion_requested_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "preferred_language",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_accessed_at",
                table: "user_tenant_accesses");

            migrationBuilder.DropColumn(
                name: "resolved_role",
                table: "telegram_login_states");

            migrationBuilder.DropColumn(
                name: "stripe_customer_id",
                table: "subscriptions");

            // The database name is read out of the old connection string; a row it can't parse fails SET NOT NULL.
            migrationBuilder.Sql("""
                ALTER TABLE tenants ADD COLUMN database_name character varying(63);
                UPDATE tenants
                SET database_name = btrim(substring(connection_string from '(?i)Database\s*=\s*([^;]+)'));
                ALTER TABLE tenants ALTER COLUMN database_name SET NOT NULL;
                ALTER TABLE tenants DROP COLUMN connection_string;
                """);

            // A used token must stay used once is_used is gone.
            migrationBuilder.Sql(
                "UPDATE impersonation_tokens SET used_at = COALESCE(used_at, created_at) WHERE is_used AND used_at IS NULL;");

            migrationBuilder.DropColumn(
                name: "is_used",
                table: "impersonation_tokens");

            // Hash the stored tokens in place (lowercase hex SHA-256, as TokenHasher.Hash) so outstanding
            // invitation and impersonation links keep working.
            migrationBuilder.Sql("""
                ALTER TABLE invitations RENAME COLUMN token TO token_hash;
                UPDATE invitations SET token_hash = encode(sha256(convert_to(token_hash, 'UTF8')), 'hex');
                ALTER TABLE invitations ALTER COLUMN token_hash TYPE character varying(64);
                ALTER INDEX ix_invitations_token RENAME TO ix_invitations_token_hash;

                ALTER TABLE impersonation_tokens RENAME COLUMN token TO token_hash;
                UPDATE impersonation_tokens SET token_hash = encode(sha256(convert_to(token_hash, 'UTF8')), 'hex');
                ALTER TABLE impersonation_tokens ALTER COLUMN token_hash TYPE character varying(64);
                ALTER INDEX ix_impersonation_tokens_token RENAME TO ix_impersonation_tokens_token_hash;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "tenants",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "rate_percent",
                table: "tenant_tax_rates",
                type: "numeric(7,4)",
                precision: 7,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "normalized_email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_name",
                table: "tenants",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_stripe_subscription_id",
                table: "subscriptions",
                column: "stripe_subscription_id",
                unique: true,
                filter: "stripe_subscription_id IS NOT NULL");

            // Not in the EF model: complex-type members can't be indexed. IftaTaxRateDuplicateChecker relies on it.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_ifta_tax_rates_year_quarter_jurisdiction
                    ON ifta_tax_rates (year, quarter, jurisdiction_country_code, jurisdiction_region)
                    NULLS NOT DISTINCT;
                """);

            migrationBuilder.Sql(
                "DELETE FROM inbound_email_routes r WHERE NOT EXISTS (SELECT 1 FROM tenants t WHERE t.id = r.tenant_id);");

            migrationBuilder.AddForeignKey(
                name: "fk_inbound_email_routes_tenant_tenant_id",
                table: "inbound_email_routes",
                column: "tenant_id",
                principalTable: "tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropColumn(
                name: "tenant_name",
                table: "telegram_login_states");

            migrationBuilder.DropColumn(
                name: "user_display_name",
                table: "telegram_login_states");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "telegram_login_states");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tenant_name",
                table: "telegram_login_states",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_display_name",
                table: "telegram_login_states",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "telegram_login_states",
                type: "uuid",
                nullable: true);

            migrationBuilder.DropForeignKey(
                name: "fk_inbound_email_routes_tenant_tenant_id",
                table: "inbound_email_routes");

            migrationBuilder.Sql("DROP INDEX IF EXISTS ux_ifta_tax_rates_year_quarter_jurisdiction;");

            migrationBuilder.DropIndex(
                name: "EmailIndex",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_tenants_name",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "ix_subscriptions_stripe_subscription_id",
                table: "subscriptions");

            // The hashes can't be reversed: links issued before the rollback stop working.
            migrationBuilder.Sql("""
                ALTER INDEX ix_invitations_token_hash RENAME TO ix_invitations_token;
                ALTER TABLE invitations RENAME COLUMN token_hash TO token;
                ALTER TABLE invitations ALTER COLUMN token TYPE character varying(128);

                ALTER INDEX ix_impersonation_tokens_token_hash RENAME TO ix_impersonation_tokens_token;
                ALTER TABLE impersonation_tokens RENAME COLUMN token_hash TO token;
                ALTER TABLE impersonation_tokens ALTER COLUMN token TYPE character varying(128);
                """);

            migrationBuilder.AddColumn<DateTime>(
                name: "deletion_requested_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_language",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_accessed_at",
                table: "user_tenant_accesses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "tenants",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AddColumn<string>(
                name: "connection_string",
                table: "tenants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "database_name",
                table: "tenants");

            migrationBuilder.AlterColumn<decimal>(
                name: "rate_percent",
                table: "tenant_tax_rates",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,4)",
                oldPrecision: 7,
                oldScale: 4);

            migrationBuilder.AddColumn<string>(
                name: "resolved_role",
                table: "telegram_login_states",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_customer_id",
                table: "subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_used",
                table: "impersonation_tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE impersonation_tokens SET is_used = true WHERE used_at IS NOT NULL;");

            migrationBuilder.CreateTable(
                name: "default_feature_configs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature = table.Column<string>(type: "text", nullable: false),
                    is_enabled_by_default = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_default_feature_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_to_email = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    stripe_invoice_id = table.Column<string>(type: "text", nullable: true),
                    tax_behavior = table.Column<string>(type: "text", nullable: false, defaultValue: "exclusive"),
                    tax_breakdown_json = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subtotal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    subtotal_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    tax_total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    tax_total_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    factoring_status = table.Column<string>(type: "text", nullable: true, defaultValue: "not_factored"),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    approval_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    total_distance_driven = table.Column<double>(type: "double precision", nullable: true),
                    total_hours_worked = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    billing_period_end = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    billing_period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_invoices", x => x.id);
                    table.ForeignKey(
                        name: "fk_invoices_users_approved_by_id",
                        column: x => x.approved_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    invoice_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reference_number = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    stripe_payment_intent_id = table.Column<string>(type: "text", nullable: true),
                    stripe_payment_method_id = table.Column<string>(type: "text", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    billing_address_city = table.Column<string>(type: "text", nullable: false),
                    billing_address_country = table.Column<string>(type: "text", nullable: false),
                    billing_address_line1 = table.Column<string>(type: "text", nullable: false),
                    billing_address_line2 = table.Column<string>(type: "text", nullable: true),
                    billing_address_state = table.Column<string>(type: "text", nullable: false),
                    billing_address_zip_code = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_user_tenant_accesses_user_id",
                table: "user_tenant_accesses",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_tokens_is_used_expires_at",
                table: "impersonation_tokens",
                columns: new[] { "is_used", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_default_feature_configs_feature",
                table: "default_feature_configs",
                column: "feature",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_invoices_approved_by_id",
                table: "invoices",
                column: "approved_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_invoices_number",
                table: "invoices",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_invoice_id",
                table: "payments",
                column: "invoice_id");
        }

        /// <summary>
        /// A DO block that raises, listing every offender, when <paramref name="query"/> (one text column)
        /// returns any rows. <paramref name="message"/> must not contain quotes or percent signs.
        /// </summary>
        private static string FailIfAny(string message, string query) => $"""
            DO $$
            DECLARE offenders text;
            BEGIN
                SELECT string_agg(offender, '; ') INTO offenders FROM ({query}) AS q(offender);
                IF offenders IS NOT NULL THEN
                    RAISE EXCEPTION 'PilotDataIntegrity pre-flight: {message}: %', offenders;
                END IF;
            END $$;
            """;
    }
}
