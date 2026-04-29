using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0004 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "can_confirm_delivery",
                table: "loads");

            migrationBuilder.RenameColumn(
                name: "can_confirm_pick_up",
                table: "loads",
                newName: "is_in_proximity");

            migrationBuilder.AddColumn<Guid>(
                name: "container_id",
                table: "loads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "destination_terminal_id",
                table: "loads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "loads",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "origin_terminal_id",
                table: "loads",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "requested_delivery_date",
                table: "loads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "requested_pickup_date",
                table: "loads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "loads",
                type: "text",
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.CreateTable(
                name: "terminals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: false),
                    address_country = table.Column<string>(type: "text", nullable: false),
                    address_line1 = table.Column<string>(type: "text", nullable: false),
                    address_line2 = table.Column<string>(type: "text", nullable: true),
                    address_state = table.Column<string>(type: "text", nullable: false),
                    address_zip_code = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_terminals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "containers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    iso_type = table.Column<string>(type: "text", nullable: false),
                    seal_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    booking_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bill_of_lading_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_laden = table.Column<bool>(type: "boolean", nullable: false),
                    gross_weight = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    current_terminal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    loaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_containers", x => x.id);
                    table.ForeignKey(
                        name: "fk_containers_terminal_current_terminal_id",
                        column: x => x.current_terminal_id,
                        principalTable: "terminals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_loads_container_id",
                table: "loads",
                column: "container_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_destination_terminal_id",
                table: "loads",
                column: "destination_terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_origin_terminal_id",
                table: "loads",
                column: "origin_terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_containers_current_terminal_id",
                table: "containers",
                column: "current_terminal_id");

            migrationBuilder.CreateIndex(
                name: "ix_containers_number",
                table: "containers",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_terminals_code",
                table: "terminals",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_loads_containers_container_id",
                table: "loads",
                column: "container_id",
                principalTable: "containers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_loads_terminal_destination_terminal_id",
                table: "loads",
                column: "destination_terminal_id",
                principalTable: "terminals",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_loads_terminal_origin_terminal_id",
                table: "loads",
                column: "origin_terminal_id",
                principalTable: "terminals",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_loads_containers_container_id",
                table: "loads");

            migrationBuilder.DropForeignKey(
                name: "fk_loads_terminal_destination_terminal_id",
                table: "loads");

            migrationBuilder.DropForeignKey(
                name: "fk_loads_terminal_origin_terminal_id",
                table: "loads");

            migrationBuilder.DropTable(
                name: "containers");

            migrationBuilder.DropTable(
                name: "terminals");

            migrationBuilder.DropIndex(
                name: "ix_loads_container_id",
                table: "loads");

            migrationBuilder.DropIndex(
                name: "ix_loads_destination_terminal_id",
                table: "loads");

            migrationBuilder.DropIndex(
                name: "ix_loads_origin_terminal_id",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "container_id",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "destination_terminal_id",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "origin_terminal_id",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "requested_delivery_date",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "requested_pickup_date",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "source",
                table: "loads");

            migrationBuilder.RenameColumn(
                name: "is_in_proximity",
                table: "loads",
                newName: "can_confirm_pick_up");

            migrationBuilder.AddColumn<bool>(
                name: "can_confirm_delivery",
                table: "loads",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
