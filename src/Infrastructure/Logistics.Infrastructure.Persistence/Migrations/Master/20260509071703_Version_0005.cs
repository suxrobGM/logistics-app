using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class Version_0005 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "anonymized_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deletion_requested_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "consent_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    anonymous_id = table.Column<Guid>(type: "uuid", nullable: true),
                    consent_type = table.Column<string>(type: "text", nullable: false),
                    granted = table.Column<bool>(type: "boolean", nullable: false),
                    timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_consent_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_consent_records_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "data_deletion_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    scheduled_for = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_deletion_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_deletion_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_export_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    blob_container = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    blob_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_export_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_export_requests_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_anonymous_id",
                table: "consent_records",
                column: "anonymous_id");

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_consent_type_timestamp",
                table: "consent_records",
                columns: new[] { "consent_type", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_user_id",
                table: "consent_records",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_deletion_requests_scheduled_for",
                table: "data_deletion_requests",
                column: "scheduled_for");

            migrationBuilder.CreateIndex(
                name: "ix_data_deletion_requests_status",
                table: "data_deletion_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_data_deletion_requests_user_id",
                table: "data_deletion_requests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_export_requests_expires_at",
                table: "data_export_requests",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_data_export_requests_requested_at",
                table: "data_export_requests",
                column: "requested_at");

            migrationBuilder.CreateIndex(
                name: "ix_data_export_requests_status",
                table: "data_export_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_data_export_requests_user_id",
                table: "data_export_requests",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consent_records");

            migrationBuilder.DropTable(
                name: "data_deletion_requests");

            migrationBuilder.DropTable(
                name: "data_export_requests");

            migrationBuilder.DropColumn(
                name: "anonymized_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "deletion_requested_at",
                table: "users");
        }
    }
}
