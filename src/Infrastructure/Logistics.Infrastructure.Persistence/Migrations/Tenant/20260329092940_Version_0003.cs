using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_api_keys_is_active",
                table: "api_keys");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "api_keys");

            migrationBuilder.CreateTable(
                name: "telegram_chats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    chat_id = table.Column<long>(type: "bigint", nullable: false),
                    chat_type = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    first_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    group_title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    notifications_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    connected_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_interaction_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telegram_chats", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_telegram_chats_chat_id",
                table: "telegram_chats",
                column: "chat_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_telegram_chats_user_id",
                table: "telegram_chats",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "telegram_chats");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "api_keys",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_api_keys_is_active",
                table: "api_keys",
                column: "is_active");
        }
    }
}
