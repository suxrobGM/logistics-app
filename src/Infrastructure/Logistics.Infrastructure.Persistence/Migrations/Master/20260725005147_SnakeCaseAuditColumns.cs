using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class SnakeCaseAuditColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "users",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "users",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "users",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "user_tenant_accesses",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "user_tenant_accesses",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "user_tenant_accesses",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "user_tenant_accesses",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "tenant_tax_rates",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "tenant_tax_rates",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "tenant_tax_rates",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tenant_tax_rates",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "tenant_feature_configs",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "tenant_feature_configs",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "tenant_feature_configs",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "tenant_feature_configs",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "subscription_plans",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "subscription_plans",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "subscription_plans",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "subscription_plans",
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
                table: "invitations",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "invitations",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "invitations",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "invitations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "ifta_tax_rates",
                newName: "updated_by");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "ifta_tax_rates",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "ifta_tax_rates",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ifta_tax_rates",
                newName: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "users",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "users",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "users",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "user_tenant_accesses",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "user_tenant_accesses",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "user_tenant_accesses",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "user_tenant_accesses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "tenant_tax_rates",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "tenant_tax_rates",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "tenant_tax_rates",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "tenant_tax_rates",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "tenant_feature_configs",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "tenant_feature_configs",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "tenant_feature_configs",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "tenant_feature_configs",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "subscription_plans",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "subscription_plans",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "subscription_plans",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "subscription_plans",
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
                table: "invitations",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "invitations",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "invitations",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "invitations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_by",
                table: "ifta_tax_rates",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "ifta_tax_rates",
                newName: "LastModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "ifta_tax_rates",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ifta_tax_rates",
                newName: "CreatedAt");
        }
    }
}
