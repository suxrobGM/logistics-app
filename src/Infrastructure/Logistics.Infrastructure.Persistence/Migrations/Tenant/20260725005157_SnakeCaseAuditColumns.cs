using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class SnakeCaseAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "trips",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "trips",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "trips",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "trips",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "tracking_links",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "tracking_links",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "tracking_links",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tracking_links",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "time_entries",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "time_entries",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "time_entries",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "time_entries",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "terminals",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "terminals",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "terminals",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "terminals",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "qbo_entity_mappings",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "qbo_entity_mappings",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "qbo_entity_mappings",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "qbo_entity_mappings",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "posted_trucks",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "posted_trucks",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "posted_trucks",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "posted_trucks",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "payments",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "payments",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "payments",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "payments",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "payment_links",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "payment_links",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "payment_links",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "payment_links",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "maintenance_schedules",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "maintenance_schedules",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "maintenance_schedules",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "maintenance_schedules",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "maintenance_records",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "maintenance_records",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "maintenance_records",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "maintenance_records",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "loads",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "loads",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "loads",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "loads",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "load_exceptions",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "load_exceptions",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "load_exceptions",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "load_exceptions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "load_condition_reports",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "load_condition_reports",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "load_condition_reports",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "load_condition_reports",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "load_board_listings",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "load_board_listings",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "load_board_listings",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "load_board_listings",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "invoices",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "invoices",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "invoices",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "invoices",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "ifta_quarter_snapshots",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "ifta_quarter_snapshots",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "ifta_quarter_snapshots",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ifta_quarter_snapshots",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "fuel_cards",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "fuel_cards",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "fuel_cards",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "fuel_cards",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "fuel_card_transactions",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "fuel_card_transactions",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "fuel_card_transactions",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "fuel_card_transactions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "expenses",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "expenses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "expenses",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "expenses",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "dvir_reports",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "dvir_reports",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "dvir_reports",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "dvir_reports",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "driver_licenses",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "driver_licenses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "driver_licenses",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "driver_licenses",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "documents",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "documents",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "documents",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "documents",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "customers",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "customers",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "customers",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "customers",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "customer_users",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "customer_users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "customer_users",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "customer_users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "containers",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "containers",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "containers",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "containers",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "broker_credit_records",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "broker_credit_records",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "broker_credit_records",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "broker_credit_records",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "ai_dispatch_sessions",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "ai_dispatch_sessions",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "ai_dispatch_sessions",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ai_dispatch_sessions",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "accident_reports",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "accident_reports",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "accident_reports",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "accident_reports",
                newName: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "trips",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "trips",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "trips",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "trips",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "tracking_links",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "tracking_links",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "tracking_links",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "tracking_links",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "time_entries",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "time_entries",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "time_entries",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "time_entries",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "terminals",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "terminals",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "terminals",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "terminals",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "qbo_entity_mappings",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "qbo_entity_mappings",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "qbo_entity_mappings",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "qbo_entity_mappings",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "posted_trucks",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "posted_trucks",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "posted_trucks",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "posted_trucks",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "payments",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "payments",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "payments",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "payments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "payment_links",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "payment_links",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "payment_links",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "payment_links",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "maintenance_schedules",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "maintenance_schedules",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "maintenance_schedules",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "maintenance_schedules",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "maintenance_records",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "maintenance_records",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "maintenance_records",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "maintenance_records",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "loads",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "loads",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "loads",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "loads",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "load_exceptions",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "load_exceptions",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "load_exceptions",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "load_exceptions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "load_condition_reports",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "load_condition_reports",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "load_condition_reports",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "load_condition_reports",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "load_board_listings",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "load_board_listings",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "load_board_listings",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "load_board_listings",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "invoices",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "invoices",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "invoices",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "invoices",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "ifta_quarter_snapshots",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "ifta_quarter_snapshots",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "ifta_quarter_snapshots",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ifta_quarter_snapshots",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "fuel_cards",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "fuel_cards",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "fuel_cards",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "fuel_cards",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "fuel_card_transactions",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "fuel_card_transactions",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "fuel_card_transactions",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "fuel_card_transactions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "expenses",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "expenses",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "expenses",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "expenses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "dvir_reports",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "dvir_reports",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "dvir_reports",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "dvir_reports",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "driver_licenses",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "driver_licenses",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "driver_licenses",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "driver_licenses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "documents",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "documents",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "documents",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "documents",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "customers",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "customers",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "customers",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "customers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "customer_users",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "customer_users",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "customer_users",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "customer_users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "containers",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "containers",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "containers",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "containers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "broker_credit_records",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "broker_credit_records",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "broker_credit_records",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "broker_credit_records",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "ai_dispatch_sessions",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "ai_dispatch_sessions",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "ai_dispatch_sessions",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ai_dispatch_sessions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "accident_reports",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "accident_reports",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "accident_reports",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "accident_reports",
                newName: "CreatedAt");
        }
    }
}
