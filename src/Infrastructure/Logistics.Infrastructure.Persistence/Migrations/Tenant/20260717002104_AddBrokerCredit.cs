using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddBrokerCredit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "broker_credit_checked_at",
                table: "load_board_listings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "broker_credit_score",
                table: "load_board_listings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "broker_days_to_pay",
                table: "load_board_listings",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "broker_credit_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mc_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    credit_score = table.Column<int>(type: "integer", nullable: true),
                    days_to_pay = table.Column<int>(type: "integer", nullable: true),
                    authority_active = table.Column<bool>(type: "boolean", nullable: true),
                    source = table.Column<string>(type: "text", nullable: false),
                    checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_broker_credit_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_broker_credit_records_mc_number",
                table: "broker_credit_records",
                column: "mc_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "broker_credit_records");

            migrationBuilder.DropColumn(
                name: "broker_credit_checked_at",
                table: "load_board_listings");

            migrationBuilder.DropColumn(
                name: "broker_credit_score",
                table: "load_board_listings");

            migrationBuilder.DropColumn(
                name: "broker_days_to_pay",
                table: "load_board_listings");
        }
    }
}
