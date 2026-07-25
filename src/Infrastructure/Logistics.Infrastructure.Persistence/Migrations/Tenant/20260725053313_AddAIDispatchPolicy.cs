using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAIDispatchPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_dispatch_policies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    generated_content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    manual_content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    decisions_analyzed = table.Column<int>(type: "integer", nullable: false),
                    model_used = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    generation_cost_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    last_decision_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_run_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_edited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_edited_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_dispatch_policies", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_dispatch_policies");
        }
    }
}
