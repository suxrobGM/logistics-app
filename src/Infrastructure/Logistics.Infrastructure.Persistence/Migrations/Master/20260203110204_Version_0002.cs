using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class Version_0002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "impersonation_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    target_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    was_successful = table.Column<bool>(type: "boolean", nullable: false),
                    failure_reason = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_impersonation_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "impersonation_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    admin_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_impersonation_tokens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_audit_logs_admin_user_id",
                table: "impersonation_audit_logs",
                column: "admin_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_audit_logs_target_user_id",
                table: "impersonation_audit_logs",
                column: "target_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_audit_logs_timestamp",
                table: "impersonation_audit_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_audit_logs_was_successful_timestamp",
                table: "impersonation_audit_logs",
                columns: new[] { "was_successful", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_tokens_expires_at",
                table: "impersonation_tokens",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_tokens_is_used_expires_at",
                table: "impersonation_tokens",
                columns: new[] { "is_used", "expires_at" });

            migrationBuilder.CreateIndex(
                name: "ix_impersonation_tokens_token",
                table: "impersonation_tokens",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "impersonation_audit_logs");

            migrationBuilder.DropTable(
                name: "impersonation_tokens");
        }
    }
}
