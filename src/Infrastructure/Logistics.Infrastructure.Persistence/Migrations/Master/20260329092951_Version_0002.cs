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
                name: "telegram_login_states",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    chat_id = table.Column<long>(type: "bigint", nullable: false),
                    chat_type = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_consumed = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    tenant_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    resolved_role = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telegram_login_states", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_telegram_login_states_state",
                table: "telegram_login_states",
                column: "state",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "telegram_login_states");
        }
    }
}
