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
            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropColumn(
                name: "method_id",
                table: "payments");

            migrationBuilder.AddColumn<string>(
                name: "stripe_payment_method_id",
                table: "payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "stripe_payment_method_id",
                table: "payments");

            migrationBuilder.AddColumn<Guid>(
                name: "method_id",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    stripe_payment_method_id = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    verification_status = table.Column<string>(type: "text", nullable: false),
                    billing_address_city = table.Column<string>(type: "text", nullable: false),
                    billing_address_country = table.Column<string>(type: "text", nullable: false),
                    billing_address_line1 = table.Column<string>(type: "text", nullable: false),
                    billing_address_line2 = table.Column<string>(type: "text", nullable: true),
                    billing_address_state = table.Column<string>(type: "text", nullable: false),
                    billing_address_zip_code = table.Column<string>(type: "text", nullable: false),
                    account_holder_name = table.Column<string>(type: "text", nullable: true),
                    account_number = table.Column<string>(type: "text", nullable: true),
                    bank_name = table.Column<string>(type: "text", nullable: true),
                    swift_code = table.Column<string>(type: "text", nullable: true),
                    card_holder_name = table.Column<string>(type: "text", nullable: true),
                    card_number = table.Column<string>(type: "text", nullable: true),
                    cvc = table.Column<string>(type: "text", nullable: true),
                    exp_month = table.Column<int>(type: "integer", nullable: true),
                    exp_year = table.Column<int>(type: "integer", nullable: true),
                    us_bank_account_payment_method_account_holder_name = table.Column<string>(type: "text", nullable: true),
                    account_holder_type = table.Column<string>(type: "text", nullable: true),
                    us_bank_account_payment_method_account_number = table.Column<string>(type: "text", nullable: true),
                    account_type = table.Column<string>(type: "text", nullable: true),
                    us_bank_account_payment_method_bank_name = table.Column<string>(type: "text", nullable: true),
                    routing_number = table.Column<string>(type: "text", nullable: true),
                    verification_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_methods", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_methods_stripe_payment_method_id",
                table: "payment_methods",
                column: "stripe_payment_method_id",
                unique: true);
        }
    }
}
