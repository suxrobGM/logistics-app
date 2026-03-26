using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dispatch_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    mode = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    triggered_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_tokens_used = table.Column<int>(type: "integer", nullable: false),
                    decision_count = table.Column<int>(type: "integer", nullable: false),
                    summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    error_message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dispatch_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dispatch_decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    reasoning = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    tool_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    tool_input = table.Column<string>(type: "text", nullable: true),
                    tool_output = table.Column<string>(type: "text", nullable: true),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dispatch_decisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_dispatch_decisions_dispatch_session_session_id",
                        column: x => x.session_id,
                        principalTable: "dispatch_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_decisions_session_id",
                table: "dispatch_decisions",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_decisions_status",
                table: "dispatch_decisions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_dispatch_sessions_number",
                table: "dispatch_sessions",
                column: "number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dispatch_decisions");

            migrationBuilder.DropTable(
                name: "dispatch_sessions");
        }
    }
}
