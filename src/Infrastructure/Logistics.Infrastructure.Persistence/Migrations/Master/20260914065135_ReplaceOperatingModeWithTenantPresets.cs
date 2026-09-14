using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <summary>
    /// Replaces <c>TenantSettings.OperatingMode</c> with admin-set <c>Tenant.Presets</c>, and adds the
    /// platform default for <c>TenantFeature.VehicleTransport</c>. Existing tenants have no row for the
    /// new feature, so the enabled default keeps their vehicle loads working.
    /// </summary>
    public partial class ReplaceOperatingModeWithTenantPresets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "presets",
                table: "tenants",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.Sql("""
                UPDATE tenants
                SET presets = CASE
                    WHEN settings_operating_mode = 'solo_operator' THEN ARRAY['general_freight', 'solo_operator']
                    ELSE ARRAY['general_freight']
                END
                """);

            migrationBuilder.DropColumn(
                name: "settings_operating_mode",
                table: "tenants");

            migrationBuilder.Sql("""
                INSERT INTO default_feature_configs (id, feature, is_enabled_by_default)
                VALUES (gen_random_uuid(), 'vehicle_transport', true)
                ON CONFLICT (feature) DO NOTHING
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM default_feature_configs WHERE feature = 'vehicle_transport'
                """);

            migrationBuilder.AddColumn<string>(
                name: "settings_operating_mode",
                table: "tenants",
                type: "text",
                nullable: false,
                defaultValue: "fleet");

            migrationBuilder.Sql("""
                UPDATE tenants
                SET settings_operating_mode = 'solo_operator'
                WHERE 'solo_operator' = ANY(presets)
                """);

            migrationBuilder.DropColumn(
                name: "presets",
                table: "tenants");
        }
    }
}
