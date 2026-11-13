using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class RenameInstantAndPositionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "impersonation_audit_logs",
                newName: "occurred_at");

            migrationBuilder.RenameIndex(
                name: "ix_impersonation_audit_logs_was_successful_timestamp",
                table: "impersonation_audit_logs",
                newName: "ix_impersonation_audit_logs_was_successful_occurred_at");

            migrationBuilder.RenameIndex(
                name: "ix_impersonation_audit_logs_timestamp",
                table: "impersonation_audit_logs",
                newName: "ix_impersonation_audit_logs_occurred_at");

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "consent_records",
                newName: "recorded_at");

            migrationBuilder.RenameIndex(
                name: "ix_consent_records_consent_type_timestamp",
                table: "consent_records",
                newName: "ix_consent_records_consent_type_recorded_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "occurred_at",
                table: "impersonation_audit_logs",
                newName: "timestamp");

            migrationBuilder.RenameIndex(
                name: "ix_impersonation_audit_logs_was_successful_occurred_at",
                table: "impersonation_audit_logs",
                newName: "ix_impersonation_audit_logs_was_successful_timestamp");

            migrationBuilder.RenameIndex(
                name: "ix_impersonation_audit_logs_occurred_at",
                table: "impersonation_audit_logs",
                newName: "ix_impersonation_audit_logs_timestamp");

            migrationBuilder.RenameColumn(
                name: "recorded_at",
                table: "consent_records",
                newName: "timestamp");

            migrationBuilder.RenameIndex(
                name: "ix_consent_records_consent_type_recorded_at",
                table: "consent_records",
                newName: "ix_consent_records_consent_type_timestamp");
        }
    }
}
