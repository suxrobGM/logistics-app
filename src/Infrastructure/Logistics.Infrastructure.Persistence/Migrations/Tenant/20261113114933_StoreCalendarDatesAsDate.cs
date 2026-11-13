using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class StoreCalendarDatesAsDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Explicit UTC: a bare cast would take the session time zone's calendar day.
            migrationBuilder.Sql("""
                ALTER TABLE maintenance_records
                    ALTER COLUMN service_date TYPE date USING (service_date AT TIME ZONE 'UTC')::date;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE invoices
                    ALTER COLUMN due_date TYPE date USING (due_date AT TIME ZONE 'UTC')::date;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE hos_logs
                    ALTER COLUMN log_date TYPE date USING (log_date AT TIME ZONE 'UTC')::date;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE expenses
                    ALTER COLUMN expense_date TYPE date USING (expense_date AT TIME ZONE 'UTC')::date,
                    ALTER COLUMN estimated_completion_date TYPE date USING (estimated_completion_date AT TIME ZONE 'UTC')::date,
                    ALTER COLUMN actual_completion_date TYPE date USING (actual_completion_date AT TIME ZONE 'UTC')::date;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE employees
                    ALTER COLUMN joined_date TYPE date USING (joined_date AT TIME ZONE 'UTC')::date;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE maintenance_records
                    ALTER COLUMN service_date TYPE timestamp with time zone USING service_date::timestamp AT TIME ZONE 'UTC';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE invoices
                    ALTER COLUMN due_date TYPE timestamp with time zone USING due_date::timestamp AT TIME ZONE 'UTC';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE hos_logs
                    ALTER COLUMN log_date TYPE timestamp with time zone USING log_date::timestamp AT TIME ZONE 'UTC';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE expenses
                    ALTER COLUMN expense_date TYPE timestamp with time zone USING expense_date::timestamp AT TIME ZONE 'UTC',
                    ALTER COLUMN estimated_completion_date TYPE timestamp with time zone USING estimated_completion_date::timestamp AT TIME ZONE 'UTC',
                    ALTER COLUMN actual_completion_date TYPE timestamp with time zone USING actual_completion_date::timestamp AT TIME ZONE 'UTC';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE employees
                    ALTER COLUMN joined_date TYPE timestamp with time zone USING joined_date::timestamp AT TIME ZONE 'UTC';
                """);
        }
    }
}
