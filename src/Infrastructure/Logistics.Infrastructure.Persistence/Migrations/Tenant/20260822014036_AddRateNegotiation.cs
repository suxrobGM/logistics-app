using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddRateNegotiation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "negotiation_id",
                table: "agent_decisions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "last_sequence",
                table: "agent_conversations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "lane_rate_floors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    origin_state = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    destination_country = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    destination_state = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    min_rate_per_mile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    min_total_rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    min_total_rate_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lane_rate_floors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rate_negotiations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_board_listing_id = table.Column<Guid>(type: "uuid", nullable: false),
                    load_id = table.Column<Guid>(type: "uuid", nullable: true),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    broker_email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    broker_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    broker_mc_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    reply_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    floor_rate_per_mile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    floor_source = table.Column<string>(type: "text", nullable: false),
                    round_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    close_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    last_sequence = table.Column<int>(type: "integer", nullable: false),
                    floor_total_rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    floor_total_rate_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    latest_broker_offer_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    latest_broker_offer_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    latest_counter_offer_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    latest_counter_offer_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rate_negotiations", x => x.id);
                    table.ForeignKey(
                        name: "fk_rate_negotiations_load_board_listings_load_board_listing_id",
                        column: x => x.load_board_listing_id,
                        principalTable: "load_board_listings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_rate_negotiations_loads_load_id",
                        column: x => x.load_id,
                        principalTable: "loads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "negotiation_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    negotiation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    direction = table.Column<string>(type: "text", nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    text_body = table.Column<string>(type: "character varying(32000)", maxLength: 32000, nullable: false),
                    raw_body = table.Column<string>(type: "character varying(65536)", maxLength: 65536, nullable: true),
                    proposed_rate_per_mile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    rfc_message_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    in_reply_to_message_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    agent_decision_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quarantined = table.Column<bool>(type: "boolean", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    proposed_total_rate_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    proposed_total_rate_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_negotiation_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_negotiation_messages_rate_negotiation_negotiation_id",
                        column: x => x.negotiation_id,
                        principalTable: "rate_negotiations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lane_rate_floors_origin_country_origin_state_destination_co",
                table: "lane_rate_floors",
                columns: new[] { "origin_country", "origin_state", "destination_country", "destination_state" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_negotiation_messages_negotiation_id_sequence",
                table: "negotiation_messages",
                columns: new[] { "negotiation_id", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_expires_at",
                table: "rate_negotiations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_load_board_listing_id",
                table: "rate_negotiations",
                column: "load_board_listing_id",
                unique: true,
                filter: "status IN ('awaiting_broker', 'broker_replied')");

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_load_id",
                table: "rate_negotiations",
                column: "load_id");

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_reply_token",
                table: "rate_negotiations",
                column: "reply_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_status",
                table: "rate_negotiations",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lane_rate_floors");

            migrationBuilder.DropTable(
                name: "negotiation_messages");

            migrationBuilder.DropTable(
                name: "rate_negotiations");

            migrationBuilder.DropColumn(
                name: "negotiation_id",
                table: "agent_decisions");

            migrationBuilder.DropColumn(
                name: "last_sequence",
                table: "agent_conversations");
        }
    }
}
