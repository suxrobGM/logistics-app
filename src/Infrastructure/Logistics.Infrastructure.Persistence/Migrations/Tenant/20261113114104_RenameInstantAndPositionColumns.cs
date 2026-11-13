using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class RenameInstantAndPositionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "truck_location_history",
                newName: "recorded_at");

            migrationBuilder.RenameIndex(
                name: "ix_truck_location_history_truck_id_timestamp",
                table: "truck_location_history",
                newName: "ix_truck_location_history_truck_id_recorded_at");

            migrationBuilder.RenameIndex(
                name: "ix_truck_location_history_timestamp",
                table: "truck_location_history",
                newName: "ix_truck_location_history_recorded_at");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "trip_stops",
                newName: "position");

            migrationBuilder.RenameColumn(
                name: "created_date",
                table: "notifications",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "ix_notifications_created_date",
                table: "notifications",
                newName: "ix_notifications_created_at");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "invoice_line_items",
                newName: "position");

            migrationBuilder.RenameColumn(
                name: "start_time",
                table: "hos_logs",
                newName: "started_at");

            migrationBuilder.RenameColumn(
                name: "end_time",
                table: "hos_logs",
                newName: "ended_at");

            migrationBuilder.RenameColumn(
                name: "accident_date_time",
                table: "accident_reports",
                newName: "occurred_at");

            migrationBuilder.RenameIndex(
                name: "ix_accident_reports_driver_id_accident_date_time",
                table: "accident_reports",
                newName: "ix_accident_reports_driver_id_occurred_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "recorded_at",
                table: "truck_location_history",
                newName: "timestamp");

            migrationBuilder.RenameIndex(
                name: "ix_truck_location_history_truck_id_recorded_at",
                table: "truck_location_history",
                newName: "ix_truck_location_history_truck_id_timestamp");

            migrationBuilder.RenameIndex(
                name: "ix_truck_location_history_recorded_at",
                table: "truck_location_history",
                newName: "ix_truck_location_history_timestamp");

            migrationBuilder.RenameColumn(
                name: "position",
                table: "trip_stops",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "notifications",
                newName: "created_date");

            migrationBuilder.RenameIndex(
                name: "ix_notifications_created_at",
                table: "notifications",
                newName: "ix_notifications_created_date");

            migrationBuilder.RenameColumn(
                name: "position",
                table: "invoice_line_items",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "started_at",
                table: "hos_logs",
                newName: "start_time");

            migrationBuilder.RenameColumn(
                name: "ended_at",
                table: "hos_logs",
                newName: "end_time");

            migrationBuilder.RenameColumn(
                name: "occurred_at",
                table: "accident_reports",
                newName: "accident_date_time");

            migrationBuilder.RenameIndex(
                name: "ix_accident_reports_driver_id_occurred_at",
                table: "accident_reports",
                newName: "ix_accident_reports_driver_id_accident_date_time");
        }
    }
}
