using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class TightenNumericAndTimeTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Time entries become start and end instants; an end at or before the start is the next day.
            migrationBuilder.Sql("""
                ALTER TABLE time_entries ADD COLUMN ended_at timestamp with time zone;
                UPDATE time_entries SET ended_at = date + end_time + CASE WHEN end_time <= start_time THEN interval '1 day' ELSE interval '0' END;
                UPDATE time_entries SET date = date + start_time;
                ALTER TABLE time_entries RENAME COLUMN date TO started_at;
                ALTER TABLE time_entries ALTER COLUMN ended_at SET NOT NULL;
                ALTER TABLE time_entries DROP COLUMN start_time, DROP COLUMN end_time;
                ALTER INDEX ix_time_entries_employee_id_date RENAME TO ix_time_entries_employee_id_started_at;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE driver_hos_statuses ADD COLUMN minutes_until_break_required integer;
                UPDATE driver_hos_statuses SET minutes_until_break_required = floor(extract(epoch FROM time_until_break_required) / 60);
                ALTER TABLE driver_hos_statuses DROP COLUMN time_until_break_required;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE maintenance_records ALTER COLUMN engine_hours TYPE numeric(10,1);
                ALTER TABLE maintenance_parts ALTER COLUMN quantity TYPE numeric(12,2);
                ALTER TABLE invoice_line_items ALTER COLUMN quantity TYPE numeric(12,2);
                ALTER TABLE expenses ALTER COLUMN quantity TYPE numeric(18,3) USING round(quantity, 3);
                ALTER TABLE agent_sessions ALTER COLUMN estimated_cost_usd TYPE numeric(18,6) USING round(estimated_cost_usd, 6);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE agent_sessions ALTER COLUMN estimated_cost_usd TYPE numeric;
                ALTER TABLE expenses ALTER COLUMN quantity TYPE numeric;
                ALTER TABLE invoice_line_items ALTER COLUMN quantity TYPE integer USING round(quantity);
                ALTER TABLE maintenance_parts ALTER COLUMN quantity TYPE integer USING round(quantity);
                ALTER TABLE maintenance_records ALTER COLUMN engine_hours TYPE integer USING round(engine_hours);
                """);

            migrationBuilder.Sql("""
                ALTER TABLE driver_hos_statuses ADD COLUMN time_until_break_required interval;
                UPDATE driver_hos_statuses SET time_until_break_required = make_interval(mins => minutes_until_break_required);
                ALTER TABLE driver_hos_statuses DROP COLUMN minutes_until_break_required;
                """);

            migrationBuilder.Sql("""
                ALTER INDEX ix_time_entries_employee_id_started_at RENAME TO ix_time_entries_employee_id_date;
                ALTER TABLE time_entries ADD COLUMN start_time interval, ADD COLUMN end_time interval;
                UPDATE time_entries SET start_time = started_at - date_trunc('day', started_at), end_time = ended_at - date_trunc('day', ended_at);
                UPDATE time_entries SET started_at = date_trunc('day', started_at);
                ALTER TABLE time_entries ALTER COLUMN start_time SET NOT NULL, ALTER COLUMN end_time SET NOT NULL;
                ALTER TABLE time_entries RENAME COLUMN started_at TO date;
                ALTER TABLE time_entries DROP COLUMN ended_at;
                """);
        }
    }
}
