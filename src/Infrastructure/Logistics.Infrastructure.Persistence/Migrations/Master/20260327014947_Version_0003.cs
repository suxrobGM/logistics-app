using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class Version_0003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "end_date",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "next_billing_date",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "start_date",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "trial_end_date",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "annual_discount_percent",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "billing_cycle_anchor",
                table: "subscription_plans");

            migrationBuilder.DropColumn(
                name: "trial_period",
                table: "subscription_plans");

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

            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                table: "subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_billing_date",
                table: "subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "start_date",
                table: "subscriptions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "trial_end_date",
                table: "subscriptions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "annual_discount_percent",
                table: "subscription_plans",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "billing_cycle_anchor",
                table: "subscription_plans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trial_period",
                table: "subscription_plans",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "method_id",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
