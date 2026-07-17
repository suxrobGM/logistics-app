using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddFuelCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_transaction_id",
                table: "expenses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fuel_card_provider",
                table: "expenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_per_unit",
                table: "expenses",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "purchase_jurisdiction_country_code",
                table: "expenses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "purchase_jurisdiction_region",
                table: "expenses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fuel_card_provider_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    api_key = table.Column<string>(type: "text", nullable: false),
                    api_secret = table.Column<string>(type: "text", nullable: true),
                    access_token = table.Column<string>(type: "text", nullable: true),
                    refresh_token = table.Column<string>(type: "text", nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    external_account_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fuel_card_provider_configurations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fuel_card_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    external_transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    quantity = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: true),
                    quantity_unit = table.Column<string>(type: "text", nullable: true),
                    price_per_unit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    product_category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    merchant_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    merchant_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    card_number_masked = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    external_card_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    unit_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    driver_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expense_id = table.Column<Guid>(type: "uuid", nullable: true),
                    raw_payload_json = table.Column<string>(type: "jsonb", nullable: true),
                    amount_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    purchase_jurisdiction_country_code = table.Column<string>(type: "text", nullable: true),
                    purchase_jurisdiction_region = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fuel_card_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_fuel_card_transactions_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "fuel_cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    external_card_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unit_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fuel_cards", x => x.id);
                    table.ForeignKey(
                        name: "fk_fuel_cards_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_expenses_fuel_card_provider_external_transaction_id",
                table: "expenses",
                columns: new[] { "fuel_card_provider", "external_transaction_id" },
                unique: true,
                filter: "external_transaction_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_provider_configurations_provider_type",
                table: "fuel_card_provider_configurations",
                column: "provider_type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_provider_type_external_transaction_id",
                table: "fuel_card_transactions",
                columns: new[] { "provider_type", "external_transaction_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_status",
                table: "fuel_card_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_truck_id_transaction_date",
                table: "fuel_card_transactions",
                columns: new[] { "truck_id", "transaction_date" });

            migrationBuilder.CreateIndex(
                name: "ix_fuel_cards_provider_type_external_card_id",
                table: "fuel_cards",
                columns: new[] { "provider_type", "external_card_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fuel_cards_truck_id",
                table: "fuel_cards",
                column: "truck_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fuel_card_provider_configurations");

            migrationBuilder.DropTable(
                name: "fuel_card_transactions");

            migrationBuilder.DropTable(
                name: "fuel_cards");

            migrationBuilder.DropIndex(
                name: "ix_expenses_fuel_card_provider_external_transaction_id",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "external_transaction_id",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "fuel_card_provider",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "price_per_unit",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "purchase_jurisdiction_country_code",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "purchase_jurisdiction_region",
                table: "expenses");
        }
    }
}
