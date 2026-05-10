using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0008 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_dispatch_decisions_dispatch_session_session_id",
                table: "dispatch_decisions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_dispatch_sessions",
                table: "dispatch_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_dispatch_decisions",
                table: "dispatch_decisions");

            migrationBuilder.RenameTable(
                name: "dispatch_sessions",
                newName: "ai_dispatch_sessions");

            migrationBuilder.RenameTable(
                name: "dispatch_decisions",
                newName: "ai_dispatch_decisions");

            migrationBuilder.RenameIndex(
                name: "ix_dispatch_sessions_number",
                table: "ai_dispatch_sessions",
                newName: "ix_ai_dispatch_sessions_number");

            migrationBuilder.RenameIndex(
                name: "ix_dispatch_decisions_status",
                table: "ai_dispatch_decisions",
                newName: "ix_ai_dispatch_decisions_status");

            migrationBuilder.RenameIndex(
                name: "ix_dispatch_decisions_session_id",
                table: "ai_dispatch_decisions",
                newName: "ix_ai_dispatch_decisions_session_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ai_dispatch_sessions",
                table: "ai_dispatch_sessions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_ai_dispatch_decisions",
                table: "ai_dispatch_decisions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_ai_dispatch_decisions_ai_dispatch_session_session_id",
                table: "ai_dispatch_decisions",
                column: "session_id",
                principalTable: "ai_dispatch_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ai_dispatch_decisions_ai_dispatch_session_session_id",
                table: "ai_dispatch_decisions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ai_dispatch_sessions",
                table: "ai_dispatch_sessions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_ai_dispatch_decisions",
                table: "ai_dispatch_decisions");

            migrationBuilder.RenameTable(
                name: "ai_dispatch_sessions",
                newName: "dispatch_sessions");

            migrationBuilder.RenameTable(
                name: "ai_dispatch_decisions",
                newName: "dispatch_decisions");

            migrationBuilder.RenameIndex(
                name: "ix_ai_dispatch_sessions_number",
                table: "dispatch_sessions",
                newName: "ix_dispatch_sessions_number");

            migrationBuilder.RenameIndex(
                name: "ix_ai_dispatch_decisions_status",
                table: "dispatch_decisions",
                newName: "ix_dispatch_decisions_status");

            migrationBuilder.RenameIndex(
                name: "ix_ai_dispatch_decisions_session_id",
                table: "dispatch_decisions",
                newName: "ix_dispatch_decisions_session_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_dispatch_sessions",
                table: "dispatch_sessions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_dispatch_decisions",
                table: "dispatch_decisions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_dispatch_decisions_dispatch_session_session_id",
                table: "dispatch_decisions",
                column: "session_id",
                principalTable: "dispatch_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
