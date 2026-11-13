using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RenameAIFeaturesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE tenant_role_claims SET claim_value = replace(claim_value, 'Permission.Dispatch.', 'Permission.AIDispatch.') WHERE claim_value LIKE 'Permission.Dispatch.%';
                UPDATE tenant_role_claims SET claim_value = replace(claim_value, 'Permission.Copilot.', 'Permission.AICopilot.') WHERE claim_value LIKE 'Permission.Copilot.%';
                UPDATE tenant_role_claims SET claim_value = replace(claim_value, 'Permission.Negotiation.', 'Permission.AINegotiation.') WHERE claim_value LIKE 'Permission.Negotiation.%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE tenant_role_claims SET claim_value = replace(claim_value, 'Permission.AIDispatch.', 'Permission.Dispatch.') WHERE claim_value LIKE 'Permission.AIDispatch.%';
                UPDATE tenant_role_claims SET claim_value = replace(claim_value, 'Permission.AICopilot.', 'Permission.Copilot.') WHERE claim_value LIKE 'Permission.AICopilot.%';
                UPDATE tenant_role_claims SET claim_value = replace(claim_value, 'Permission.AINegotiation.', 'Permission.Negotiation.') WHERE claim_value LIKE 'Permission.AINegotiation.%';
                """);
        }
    }
}
