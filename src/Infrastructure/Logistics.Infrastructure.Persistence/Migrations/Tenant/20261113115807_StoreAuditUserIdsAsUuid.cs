using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class StoreAuditUserIdsAsUuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE accident_reports
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE agent_conversations
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE agent_sessions
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ai_dispatch_policies
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE broker_credit_records
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE cash_advances
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE containers
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE customer_users
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE customers
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE documents
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE driver_licenses
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE dvir_reports
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE expenses
                    ALTER COLUMN approved_by_id TYPE uuid USING NULLIF(approved_by_id, '')::uuid,
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE factoring_batch_items
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE factoring_batches
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE factoring_companies
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE fuel_card_transactions
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE fuel_cards
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ifta_quarter_snapshots
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE invoices
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE lane_rate_floors
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE load_board_listings
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE load_condition_reports
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE load_exceptions
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE loads
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE maintenance_records
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE payment_links
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE payments
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE posted_trucks
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE qbo_entity_mappings
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE rate_negotiations
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE terminals
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE time_entries
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE tracking_links
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE trailers
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE trips
                    ALTER COLUMN created_by TYPE uuid USING NULLIF(created_by, '')::uuid,
                    ALTER COLUMN updated_by TYPE uuid USING NULLIF(updated_by, '')::uuid;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE accident_reports
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE agent_conversations
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE agent_sessions
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ai_dispatch_policies
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE broker_credit_records
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE cash_advances
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE containers
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE customer_users
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE customers
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE documents
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE driver_licenses
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE dvir_reports
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE expenses
                    ALTER COLUMN approved_by_id TYPE character varying(50) USING approved_by_id::text,
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE factoring_batch_items
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE factoring_batches
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE factoring_companies
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE fuel_card_transactions
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE fuel_cards
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ifta_quarter_snapshots
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE invoices
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE lane_rate_floors
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE load_board_listings
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE load_condition_reports
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE load_exceptions
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE loads
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE maintenance_records
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE payment_links
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE payments
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE posted_trucks
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE qbo_entity_mappings
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE rate_negotiations
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE terminals
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE time_entries
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE tracking_links
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE trailers
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE trips
                    ALTER COLUMN created_by TYPE character varying(50) USING created_by::text,
                    ALTER COLUMN updated_by TYPE character varying(50) USING updated_by::text;
                """);
        }
    }
}
