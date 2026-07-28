using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <summary>
    /// Platform default for <c>TenantFeature.AICopilot</c>. Off by default - the copilot is a
    /// plan differentiator granted to Professional and Enterprise via <c>PlanFeature</c>; other
    /// tenants opt in per tenant. Data-only, so there is no model snapshot diff.
    /// </summary>
    public partial class AddAICopilotFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: the unique index on feature turns a re-run into a no-op.
            migrationBuilder.Sql("""
                INSERT INTO default_feature_configs (id, feature, is_enabled_by_default)
                VALUES (gen_random_uuid(), 'AICopilot', false)
                ON CONFLICT (feature) DO NOTHING
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM default_feature_configs WHERE feature = 'AICopilot'
                """);
        }
    }
}
