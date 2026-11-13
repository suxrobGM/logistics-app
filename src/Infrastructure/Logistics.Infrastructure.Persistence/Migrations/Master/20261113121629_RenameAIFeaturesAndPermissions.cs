using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class RenameAIFeaturesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE plan_features SET feature = 'ai_dispatch' WHERE feature = 'agentic_dispatch';
                UPDATE plan_features SET feature = 'ai_negotiation' WHERE feature = 'ai_rate_negotiation';
                UPDATE tenant_feature_configs SET feature = 'ai_dispatch' WHERE feature = 'agentic_dispatch';
                UPDATE tenant_feature_configs SET feature = 'ai_negotiation' WHERE feature = 'ai_rate_negotiation';
                """);

            migrationBuilder.Sql("""
                UPDATE role_claims SET claim_value = replace(claim_value, 'Permission.Dispatch.', 'Permission.AIDispatch.') WHERE claim_value LIKE 'Permission.Dispatch.%';
                UPDATE role_claims SET claim_value = replace(claim_value, 'Permission.Copilot.', 'Permission.AICopilot.') WHERE claim_value LIKE 'Permission.Copilot.%';
                UPDATE role_claims SET claim_value = replace(claim_value, 'Permission.Negotiation.', 'Permission.AINegotiation.') WHERE claim_value LIKE 'Permission.Negotiation.%';
                UPDATE user_claims SET claim_value = replace(claim_value, 'Permission.Dispatch.', 'Permission.AIDispatch.') WHERE claim_value LIKE 'Permission.Dispatch.%';
                UPDATE user_claims SET claim_value = replace(claim_value, 'Permission.Copilot.', 'Permission.AICopilot.') WHERE claim_value LIKE 'Permission.Copilot.%';
                UPDATE user_claims SET claim_value = replace(claim_value, 'Permission.Negotiation.', 'Permission.AINegotiation.') WHERE claim_value LIKE 'Permission.Negotiation.%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE plan_features SET feature = 'agentic_dispatch' WHERE feature = 'ai_dispatch';
                UPDATE plan_features SET feature = 'ai_rate_negotiation' WHERE feature = 'ai_negotiation';
                UPDATE tenant_feature_configs SET feature = 'agentic_dispatch' WHERE feature = 'ai_dispatch';
                UPDATE tenant_feature_configs SET feature = 'ai_rate_negotiation' WHERE feature = 'ai_negotiation';
                """);

            migrationBuilder.Sql("""
                UPDATE role_claims SET claim_value = replace(claim_value, 'Permission.AIDispatch.', 'Permission.Dispatch.') WHERE claim_value LIKE 'Permission.AIDispatch.%';
                UPDATE role_claims SET claim_value = replace(claim_value, 'Permission.AICopilot.', 'Permission.Copilot.') WHERE claim_value LIKE 'Permission.AICopilot.%';
                UPDATE role_claims SET claim_value = replace(claim_value, 'Permission.AINegotiation.', 'Permission.Negotiation.') WHERE claim_value LIKE 'Permission.AINegotiation.%';
                UPDATE user_claims SET claim_value = replace(claim_value, 'Permission.AIDispatch.', 'Permission.Dispatch.') WHERE claim_value LIKE 'Permission.AIDispatch.%';
                UPDATE user_claims SET claim_value = replace(claim_value, 'Permission.AICopilot.', 'Permission.Copilot.') WHERE claim_value LIKE 'Permission.AICopilot.%';
                UPDATE user_claims SET claim_value = replace(claim_value, 'Permission.AINegotiation.', 'Permission.Negotiation.') WHERE claim_value LIKE 'Permission.AINegotiation.%';
                """);
        }
    }
}
