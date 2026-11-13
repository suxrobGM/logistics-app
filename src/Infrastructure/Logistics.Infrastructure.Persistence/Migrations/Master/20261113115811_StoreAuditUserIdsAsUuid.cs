using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class StoreAuditUserIdsAsUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE ifta_tax_rates
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE invitations
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE subscription_plans
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE tenant_feature_configs
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE tenant_tax_rates
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE user_tenant_accesses
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE users
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE ifta_tax_rates
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE invitations
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE subscription_plans
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE tenant_feature_configs
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE tenant_tax_rates
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE user_tenant_accesses
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE users
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);
        }
    }
}
