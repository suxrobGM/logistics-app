using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAgentAutoApprove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "auto_approve_streak",
                table: "ai_dispatch_policies",
                type: "integer",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddColumn<int[]>(
                name: "auto_approve_types",
                table: "ai_dispatch_policies",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int>(
                name: "minutes_saved_per_decision",
                table: "ai_dispatch_policies",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<bool>(
                name: "auto_approved",
                table: "agent_decisions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "reverted_at",
                table: "agent_decisions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "auto_approve_streak",
                table: "ai_dispatch_policies");

            migrationBuilder.DropColumn(
                name: "auto_approve_types",
                table: "ai_dispatch_policies");

            migrationBuilder.DropColumn(
                name: "minutes_saved_per_decision",
                table: "ai_dispatch_policies");

            migrationBuilder.DropColumn(
                name: "auto_approved",
                table: "agent_decisions");

            migrationBuilder.DropColumn(
                name: "reverted_at",
                table: "agent_decisions");
        }
    }
}
