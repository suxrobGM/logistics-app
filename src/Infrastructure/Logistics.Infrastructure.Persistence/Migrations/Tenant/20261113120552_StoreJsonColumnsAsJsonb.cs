using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class StoreJsonColumnsAsJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE invoices
                    ALTER COLUMN tax_breakdown_json TYPE jsonb USING tax_breakdown_json::jsonb;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE agent_decisions
                    ALTER COLUMN tool_input TYPE jsonb USING tool_input::jsonb,
                    ALTER COLUMN tool_output TYPE jsonb USING tool_output::jsonb;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE invoices
                    ALTER COLUMN tax_breakdown_json TYPE text USING tax_breakdown_json::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE agent_decisions
                    ALTER COLUMN tool_input TYPE text USING tool_input::text,
                    ALTER COLUMN tool_output TYPE text USING tool_output::text;
                """);
        }
    }
}
