using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddLoadAppointmentsAndStopEta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "eta_updated_at",
                table: "trips",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "hos_shortfall_flagged_at",
                table: "trips",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "estimated_arrival_at",
                table: "trip_stops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "late_notified_at",
                table: "trip_stops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "delivery_appointment_from",
                table: "loads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "delivery_appointment_to",
                table: "loads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "pickup_appointment_from",
                table: "loads",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "pickup_appointment_to",
                table: "loads",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "eta_updated_at",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "hos_shortfall_flagged_at",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "estimated_arrival_at",
                table: "trip_stops");

            migrationBuilder.DropColumn(
                name: "late_notified_at",
                table: "trip_stops");

            migrationBuilder.DropColumn(
                name: "delivery_appointment_from",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "delivery_appointment_to",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "pickup_appointment_from",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "pickup_appointment_to",
                table: "loads");
        }
    }
}
