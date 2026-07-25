using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <summary>
    /// Platform default for <c>TenantFeature.IntermodalContainers</c>. Off by default - most fleets
    /// do not run drayage, and the point of the flag is keeping ~510 tokens off their dispatch
    /// requests. Professional and Enterprise grant it via <c>PlanFeature</c>; others opt in per tenant.
    /// Data-only, so there is no model snapshot diff.
    /// </summary>
    public partial class AddIntermodalContainersFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotent: the unique index on feature turns a re-run into a no-op.
            migrationBuilder.Sql("""
                INSERT INTO default_feature_configs (id, feature, is_enabled_by_default)
                VALUES (gen_random_uuid(), 'IntermodalContainers', false)
                ON CONFLICT (feature) DO NOTHING
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM default_feature_configs WHERE feature = 'IntermodalContainers'
                """);
        }
    }
}
