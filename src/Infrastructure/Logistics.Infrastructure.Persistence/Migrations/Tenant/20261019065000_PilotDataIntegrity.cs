using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class PilotDataIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS get_company_stats();
                DROP FUNCTION IF EXISTS get_trucks_stats(timestamp, timestamp, integer, integer, varchar);
                """);

            // Data steps that read columns dropped below or must precede a new unique index or FK.
            // TruckExpense kept its truck in truck_expense_truck_id; both TPH siblings now share truck_id.
            migrationBuilder.Sql("""
                UPDATE expenses SET truck_id = truck_expense_truck_id
                WHERE type = 'truck' AND truck_expense_truck_id IS NOT NULL;

                DELETE FROM invoices WHERE type = 'subscription';

                UPDATE loads SET source = 'load_board'
                WHERE external_source_provider IS NOT NULL AND source <> 'load_board';

                UPDATE tracking_links SET created_by = created_by_user_id::text WHERE created_by IS NULL;
                UPDATE load_exceptions SET created_by = reported_by_id::text
                WHERE created_by IS NULL AND reported_by_id <> '00000000-0000-0000-0000-000000000000';

                UPDATE agent_sessions SET created_at = started_at WHERE created_at <> started_at;

                UPDATE driver_licenses SET last_reminder_threshold_days = NULL
                WHERE last_reminder_threshold_days IS NOT NULL
                  AND expires_at > now() + make_interval(days => last_reminder_threshold_days);
                """);

            // Lossless duplicate cleanup ahead of the unique indexes.
            migrationBuilder.Sql("""
                UPDATE customer_users c SET is_active = false
                FROM (SELECT id, row_number() OVER (PARTITION BY user_id ORDER BY created_at DESC, id DESC) AS rn
                      FROM customer_users WHERE is_active) r
                WHERE c.id = r.id AND r.rn > 1;

                DELETE FROM hos_logs l USING hos_logs k
                WHERE l.employee_id = k.employee_id AND l.provider_type = k.provider_type
                  AND l.external_log_id = k.external_log_id AND l.id > k.id;

                WITH ranked AS (
                    SELECT id, row_number() OVER (
                        PARTITION BY provider_type, external_violation_id
                        ORDER BY is_resolved DESC, violation_date, id) AS rn
                    FROM hos_violations WHERE external_violation_id IS NOT NULL
                )
                DELETE FROM hos_violations v USING ranked r WHERE v.id = r.id AND r.rn > 1;

                WITH ranked AS (
                    SELECT id,
                           first_value(id) OVER w AS keep_id,
                           sum(miles) OVER (PARTITION BY truck_id, date, jurisdiction_country_code, jurisdiction_region) AS total
                    FROM truck_jurisdiction_mileage
                    WINDOW w AS (PARTITION BY truck_id, date, jurisdiction_country_code, jurisdiction_region ORDER BY id)
                )
                UPDATE truck_jurisdiction_mileage m SET miles = r.total
                FROM ranked r WHERE m.id = r.id AND r.id = r.keep_id;

                DELETE FROM truck_jurisdiction_mileage m
                USING truck_jurisdiction_mileage k
                WHERE m.truck_id = k.truck_id AND m.date = k.date
                  AND m.jurisdiction_country_code = k.jurisdiction_country_code
                  AND m.jurisdiction_region IS NOT DISTINCT FROM k.jurisdiction_region
                  AND m.id > k.id;

                UPDATE trucks SET vin = NULLIF(upper(btrim(vin)), '') WHERE vin IS DISTINCT FROM NULLIF(upper(btrim(vin)), '');
                UPDATE trailers SET vin = NULLIF(upper(btrim(vin)), '') WHERE vin IS DISTINCT FROM NULLIF(upper(btrim(vin)), '');
                UPDATE trucks SET number = btrim(number) WHERE number <> btrim(number);
                UPDATE trucks SET secondary_driver_id = NULL WHERE secondary_driver_id = main_driver_id;

                UPDATE condition_defects SET
                    part_category = lower(regexp_replace(part_category, '([a-z])([A-Z])', '\1_\2', 'g')),
                    severity = lower(regexp_replace(severity, '([a-z])([A-Z])', '\1_\2', 'g')),
                    zone = lower(regexp_replace(zone, '([a-z])([A-Z])', '\1_\2', 'g')),
                    damage_type = lower(regexp_replace(damage_type, '([a-z])([A-Z])', '\1_\2', 'g'))
                WHERE part_category ~ '[A-Z]' OR severity ~ '[A-Z]' OR zone ~ '[A-Z]' OR damage_type ~ '[A-Z]';
                """);

            // Dangling ids the new foreign keys would reject.
            migrationBuilder.Sql("""
                UPDATE load_accessorials a SET trip_stop_id = NULL
                WHERE trip_stop_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM trip_stops s WHERE s.id = a.trip_stop_id);
                UPDATE condition_defects d SET photo_document_id = NULL
                WHERE photo_document_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM documents x WHERE x.id = d.photo_document_id);
                UPDATE fuel_card_transactions t SET expense_id = NULL
                WHERE expense_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM expenses e WHERE e.id = t.expense_id);
                UPDATE agent_messages m SET session_id = NULL
                WHERE session_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM agent_sessions s WHERE s.id = m.session_id);
                UPDATE negotiation_messages m SET agent_decision_id = NULL
                WHERE agent_decision_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM agent_decisions d WHERE d.id = m.agent_decision_id);
                UPDATE rate_negotiations n SET conversation_id = NULL
                WHERE conversation_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM agent_conversations c WHERE c.id = n.conversation_id);
                """);

            // Payments: drop the SaaS charges invoice.paid wrote, link Checkout payments saved unlinked, then
            // recompute invoice status from settled payments only (Invoice.RecalculatePaymentStatus). The
            // factoring FK cleanup runs after the delete so it also catches items that pointed at those rows.
            migrationBuilder.Sql("""
                DELETE FROM payments
                WHERE invoice_id IS NULL
                  AND stripe_payment_intent_id IS NULL
                  AND description IS NULL
                  AND reference_number IS NULL
                  AND amount_currency = lower(amount_currency);

                UPDATE factoring_batch_items i SET payment_id = NULL
                WHERE payment_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM payments p WHERE p.id = i.payment_id);

                UPDATE payments p SET invoice_id = i.id
                FROM invoices i
                WHERE p.invoice_id IS NULL
                  AND p.stripe_payment_intent_id LIKE 'pi\_%'
                  AND p.description = 'Payment for Invoice #' || i.number;

                WITH paid AS (
                    SELECT i.id,
                           COALESCE(SUM(p.amount_amount) FILTER (
                               WHERE p.status = 'paid' AND upper(p.amount_currency) = upper(i.total_currency)), 0) AS amount_paid
                    FROM invoices i
                    LEFT JOIN payments p ON p.invoice_id = i.id
                    GROUP BY i.id
                )
                UPDATE invoices i
                SET status = CASE
                        WHEN paid.amount_paid >= i.total_amount AND paid.amount_paid > 0 THEN 'paid'
                        WHEN paid.amount_paid > 0 THEN 'partially_paid'
                        WHEN i.type = 'payroll' THEN 'approved'
                        WHEN i.due_date < now() THEN 'overdue'
                        WHEN i.sent_at IS NOT NULL THEN 'sent'
                        ELSE 'issued'
                    END
                FROM paid
                WHERE paid.id = i.id
                  AND i.status NOT IN ('draft', 'pending_approval', 'rejected', 'cancelled')
                  AND (i.status IN ('paid', 'partially_paid') OR paid.amount_paid > 0);

                UPDATE invoices i
                SET customer_id = l.customer_id
                FROM loads l
                WHERE i.load_id = l.id
                  AND i.type = 'load'
                  AND i.customer_id IS DISTINCT FROM l.customer_id
                  AND i.status NOT IN ('paid', 'cancelled');
                """);

            // Housekeeping: message counters never below the highest stored sequence; chats of deleted employees.
            migrationBuilder.Sql("""
                UPDATE agent_conversations c SET last_sequence = m.max_sequence
                FROM (SELECT conversation_id, max(sequence) AS max_sequence FROM agent_messages GROUP BY conversation_id) m
                WHERE m.conversation_id = c.id AND c.last_sequence < m.max_sequence;

                UPDATE rate_negotiations n SET last_sequence = m.max_sequence
                FROM (SELECT negotiation_id, max(sequence) AS max_sequence FROM negotiation_messages GROUP BY negotiation_id) m
                WHERE m.negotiation_id = n.id AND n.last_sequence < m.max_sequence;

                DELETE FROM telegram_chats t
                WHERE t.user_id IS NULL OR NOT EXISTS (SELECT 1 FROM employees e WHERE e.id = t.user_id);
                """);

            // Pre-flights: what is left can't be fixed without losing or misattributing business data, so
            // the migration stops and lists the offenders. Fix them by hand and rerun.
            migrationBuilder.Sql(FailIfAny(
                "Loads flagged hazmat have no hazmat class and would silently lose their hazmat checks; set the class from the paperwork or clear is_hazmat by hand",
                "SELECT 'load ' || number FROM loads WHERE is_hazmat AND hazmat_class IS NULL"));

            migrationBuilder.Sql(FailIfAny(
                "Truck or body shop expenses have no truck",
                "SELECT type || ' expense ' || id FROM expenses WHERE type IN ('truck', 'body_shop') AND truck_id IS NULL"));

            migrationBuilder.Sql(FailIfAny(
                "Customer names repeat regardless of case; merge or rename them by hand",
                "SELECT lower(name) || ' x' || count(*) FROM customers GROUP BY lower(name) HAVING count(*) > 1"));

            migrationBuilder.Sql(FailIfAny(
                "Employee emails repeat regardless of case; fix the stale email by hand",
                "SELECT lower(email) || ' x' || count(*) FROM employees GROUP BY lower(email) HAVING count(*) > 1"));

            migrationBuilder.Sql(FailIfAny(
                "Loads share an external source id within one source; clear it on all but the oldest by hand",
                """
                SELECT source || '/' || external_source_id || ': loads ' || string_agg(number::text, ', ' ORDER BY number)
                FROM loads WHERE external_source_id IS NOT NULL
                GROUP BY source, external_source_id HAVING count(*) > 1
                """));

            migrationBuilder.Sql(FailIfAny(
                "Trip stops carry more than one detention line; delete the extras by hand",
                """
                SELECT 'trip stop ' || trip_stop_id || ' x' || count(*) FROM load_accessorials
                WHERE line_item_type = 'detention' AND trip_stop_id IS NOT NULL
                GROUP BY trip_stop_id HAVING count(*) > 1
                """));

            migrationBuilder.Sql(FailIfAny(
                "VINs are too long or shared by two vehicles; correct the typo by hand",
                """
                SELECT 'truck ' || number || ' VIN over 17 chars' FROM trucks WHERE length(vin) > 17
                UNION ALL
                SELECT 'truck VIN ' || vin || ' on ' || string_agg(number, ', ') FROM trucks
                WHERE vin IS NOT NULL GROUP BY vin HAVING count(*) > 1
                UNION ALL
                SELECT 'trailer VIN ' || vin || ' on ' || string_agg(number, ', ') FROM trailers
                WHERE vin IS NOT NULL GROUP BY vin HAVING count(*) > 1
                """));

            migrationBuilder.Sql(FailIfAny(
                "Truck numbers are over 50 characters or repeat regardless of case; rename them by hand",
                """
                SELECT 'truck ' || id || ' number over 50 chars' FROM trucks WHERE length(number) > 50
                UNION ALL
                SELECT lower(number) || ' x' || count(*) FROM trucks GROUP BY lower(number) HAVING count(*) > 1
                """));

            migrationBuilder.Sql(FailIfAny(
                "Drivers hold seats on more than one truck; clear the extra seats by hand",
                """
                SELECT 'driver ' || d.driver_id || ': ' || string_agg(d.number || ' (' || d.seat || ')', ', ')
                FROM (
                    SELECT main_driver_id AS driver_id, number, 'main' AS seat FROM trucks WHERE main_driver_id IS NOT NULL
                    UNION ALL
                    SELECT secondary_driver_id, number, 'secondary' FROM trucks WHERE secondary_driver_id IS NOT NULL
                ) d
                GROUP BY d.driver_id HAVING count(*) > 1
                """));

            migrationBuilder.DropForeignKey(
                name: "fk_documents_accident_reports_accident_report_id",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "fk_documents_dvir_reports_dvir_report_id",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "fk_documents_maintenance_records_maintenance_record_id",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "fk_expenses_trucks_truck_id1",
                table: "expenses");

            migrationBuilder.DropForeignKey(
                name: "fk_maintenance_records_maintenance_schedule_maintenance_schedu",
                table: "maintenance_records");

            migrationBuilder.DropTable(
                name: "driver_behavior_events");

            migrationBuilder.DropTable(
                name: "maintenance_schedules");

            migrationBuilder.DropIndex(
                name: "ix_trucks_main_driver_id",
                table: "trucks");

            migrationBuilder.DropIndex(
                name: "ix_trucks_number",
                table: "trucks");

            migrationBuilder.DropIndex(
                name: "ix_trucks_secondary_driver_id",
                table: "trucks");

            migrationBuilder.DropIndex(
                name: "ix_maintenance_records_maintenance_schedule_id",
                table: "maintenance_records");

            migrationBuilder.DropIndex(
                name: "ix_hos_violations_external_violation_id",
                table: "hos_violations");

            migrationBuilder.DropIndex(
                name: "ix_hos_logs_external_log_id",
                table: "hos_logs");

            migrationBuilder.DropIndex(
                name: "ix_expenses_truck_id1",
                table: "expenses");

            migrationBuilder.DropIndex(
                name: "ix_documents_accident_report_id",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "ix_documents_dvir_report_id",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "ix_documents_maintenance_record_id",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "ix_customer_users_user_id",
                table: "customer_users");

            migrationBuilder.DropIndex(
                name: "ix_agent_sessions_started_at",
                table: "agent_sessions");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "tracking_links");

            migrationBuilder.DropColumn(
                name: "first_name",
                table: "telegram_chats");

            migrationBuilder.DropColumn(
                name: "group_title",
                table: "telegram_chats");

            migrationBuilder.DropColumn(
                name: "last_interaction_at",
                table: "telegram_chats");

            migrationBuilder.DropColumn(
                name: "role",
                table: "telegram_chats");

            migrationBuilder.DropColumn(
                name: "username",
                table: "telegram_chats");

            migrationBuilder.DropColumn(
                name: "recorded_by_user_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "is_read",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "maintenance_schedule_id",
                table: "maintenance_records");

            migrationBuilder.DropColumn(
                name: "external_broker_reference",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "external_source_provider",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "is_hazmat",
                table: "loads");

            migrationBuilder.DropColumn(
                name: "reported_by_id",
                table: "load_exceptions");

            migrationBuilder.DropColumn(
                name: "raw_json",
                table: "load_board_listings");

            migrationBuilder.DropColumn(
                name: "external_account_id",
                table: "load_board_configurations");

            migrationBuilder.DropColumn(
                name: "billing_period_end",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "billing_period_start",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "stripe_invoice_id",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "subscription_id",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "truck_expense_truck_id",
                table: "expenses");

            migrationBuilder.DropColumn(
                name: "last_reminder_sent_at",
                table: "driver_licenses");

            migrationBuilder.DropColumn(
                name: "external_driver_id",
                table: "driver_hos_statuses");

            migrationBuilder.DropColumn(
                name: "provider_type",
                table: "driver_hos_statuses");

            migrationBuilder.DropColumn(
                name: "accident_report_id",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "dvir_report_id",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "maintenance_record_id",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "status",
                table: "documents");

            migrationBuilder.DropColumn(
                name: "last_login_at",
                table: "customer_users");

            migrationBuilder.DropColumn(
                name: "generation_cost_usd",
                table: "ai_dispatch_policies");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "agent_sessions");

            migrationBuilder.DropColumn(
                name: "summary",
                table: "agent_sessions");

            migrationBuilder.AlterColumn<string>(
                name: "vin",
                table: "trucks",
                type: "character varying(17)",
                maxLength: 17,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "number",
                table: "trucks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "driver_id",
                table: "trips",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "secondary_driver_id",
                table: "trips",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_transfer_id",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE payments
                SET stripe_transfer_id = stripe_payment_intent_id, stripe_payment_intent_id = NULL
                WHERE stripe_payment_intent_id LIKE 'tr\_%';
                """);

            migrationBuilder.Sql(FailIfAny(
                "Payments share a Stripe payment intent or transfer id; keep the oldest row per id and delete the rest by hand",
                """
                SELECT 'intent ' || stripe_payment_intent_id || ' x' || count(*) FROM payments
                WHERE stripe_payment_intent_id IS NOT NULL GROUP BY stripe_payment_intent_id HAVING count(*) > 1
                UNION ALL
                SELECT 'transfer ' || stripe_transfer_id || ' x' || count(*) FROM payments
                WHERE stripe_transfer_id IS NOT NULL GROUP BY stripe_transfer_id HAVING count(*) > 1
                """));

            migrationBuilder.Sql("""
                ALTER TABLE invoices ALTER COLUMN period_start TYPE date USING (period_start AT TIME ZONE 'UTC')::date;
                ALTER TABLE invoices ALTER COLUMN period_end TYPE date USING (period_end AT TIME ZONE 'UTC')::date;
                """);

            migrationBuilder.AlterColumn<decimal>(
                name: "tax_rate_percent",
                table: "invoice_line_items",
                type: "numeric(7,4)",
                precision: 7,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            // The portal sent the local midnight of the picked day; +12h rounds to the nearest UTC midnight,
            // right for any offset within +-11h. Runs after the reminder reset, which compares against now().
            migrationBuilder.Sql("""
                ALTER TABLE driver_licenses ALTER COLUMN issued_date TYPE date
                    USING (issued_date AT TIME ZONE 'UTC' + interval '12 hours')::date;
                ALTER TABLE driver_licenses ALTER COLUMN expires_at TYPE date
                    USING (expires_at AT TIME ZONE 'UTC' + interval '12 hours')::date;
                ALTER TABLE driver_licenses ALTER COLUMN medical_cert_expires_at TYPE date
                    USING (medical_cert_expires_at AT TIME ZONE 'UTC' + interval '12 hours')::date;
                """);

            migrationBuilder.CreateTable(
                name: "notification_reads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    notification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_reads", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_reads_notifications_notification_id",
                        column: x => x.notification_id,
                        principalTable: "notifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trucks_main_driver_id",
                table: "trucks",
                column: "main_driver_id",
                unique: true,
                filter: "main_driver_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_trucks_secondary_driver_id",
                table: "trucks",
                column: "secondary_driver_id",
                unique: true,
                filter: "secondary_driver_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_trucks_vin",
                table: "trucks",
                column: "vin",
                unique: true,
                filter: "vin IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_trucks_main_driver_not_secondary",
                table: "trucks",
                sql: "main_driver_id <> secondary_driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_driver_id",
                table: "trips",
                column: "driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_secondary_driver_id",
                table: "trips",
                column: "secondary_driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_trips_status",
                table: "trips",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_trailers_vin",
                table: "trailers",
                column: "vin",
                unique: true,
                filter: "vin IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_rate_negotiations_conversation_id",
                table: "rate_negotiations",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_stripe_payment_intent_id",
                table: "payments",
                column: "stripe_payment_intent_id",
                unique: true,
                filter: "stripe_payment_intent_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_payments_stripe_transfer_id",
                table: "payments",
                column: "stripe_transfer_id",
                unique: true,
                filter: "stripe_transfer_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_created_date",
                table: "notifications",
                column: "created_date");

            migrationBuilder.CreateIndex(
                name: "ix_negotiation_messages_agent_decision_id",
                table: "negotiation_messages",
                column: "agent_decision_id");

            migrationBuilder.CreateIndex(
                name: "ix_loads_source_external_source_id",
                table: "loads",
                columns: new[] { "source", "external_source_id" },
                unique: true,
                filter: "external_source_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_loads_status",
                table: "loads",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_load_accessorials_trip_stop_id",
                table: "load_accessorials",
                column: "trip_stop_id",
                unique: true,
                filter: "line_item_type = 'detention'");

            migrationBuilder.CreateIndex(
                name: "ix_hos_violations_provider_type_external_violation_id",
                table: "hos_violations",
                columns: new[] { "provider_type", "external_violation_id" },
                unique: true,
                filter: "external_violation_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_hos_logs_employee_id_provider_type_external_log_id",
                table: "hos_logs",
                columns: new[] { "employee_id", "provider_type", "external_log_id" },
                unique: true,
                filter: "external_log_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_fuel_card_transactions_expense_id",
                table: "fuel_card_transactions",
                column: "expense_id");

            migrationBuilder.CreateIndex(
                name: "ix_factoring_batch_items_payment_id",
                table: "factoring_batch_items",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_users_user_id",
                table: "customer_users",
                column: "user_id",
                unique: true,
                filter: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_condition_defects_photo_document_id",
                table: "condition_defects",
                column: "photo_document_id");

            migrationBuilder.CreateIndex(
                name: "ix_agent_sessions_created_at",
                table: "agent_sessions",
                column: "created_at")
                .Annotation("Npgsql:IndexInclude", new[] { "estimated_cost_usd" });

            migrationBuilder.CreateIndex(
                name: "ix_agent_messages_session_id",
                table: "agent_messages",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_reads_notification_id_user_id",
                table: "notification_reads",
                columns: new[] { "notification_id", "user_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_agent_messages_agent_session_session_id",
                table: "agent_messages",
                column: "session_id",
                principalTable: "agent_sessions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_condition_defects_document_photo_document_id",
                table: "condition_defects",
                column: "photo_document_id",
                principalTable: "documents",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_factoring_batch_items_payment_payment_id",
                table: "factoring_batch_items",
                column: "payment_id",
                principalTable: "payments",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_fuel_card_transactions_expenses_expense_id",
                table: "fuel_card_transactions",
                column: "expense_id",
                principalTable: "expenses",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_load_accessorials_trip_stop_trip_stop_id",
                table: "load_accessorials",
                column: "trip_stop_id",
                principalTable: "trip_stops",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_negotiation_messages_agent_decisions_agent_decision_id",
                table: "negotiation_messages",
                column: "agent_decision_id",
                principalTable: "agent_decisions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_rate_negotiations_agent_conversations_conversation_id",
                table: "rate_negotiations",
                column: "conversation_id",
                principalTable: "agent_conversations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_trips_employees_driver_id",
                table: "trips",
                column: "driver_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_trips_employees_secondary_driver_id",
                table: "trips",
                column: "secondary_driver_id",
                principalTable: "employees",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // Not in the EF model (expression and complex-type indexes). The mileage index must match
            // TruckLocationRecorder's ON CONFLICT target exactly.
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_customers_name_lower ON customers (lower(name));
                CREATE UNIQUE INDEX ux_employees_email_lower ON employees (lower(email));
                CREATE UNIQUE INDEX ux_trucks_number_lower ON trucks (lower(number));
                CREATE UNIQUE INDEX ux_truck_jurisdiction_mileage_truck_date_jurisdiction
                    ON truck_jurisdiction_mileage (truck_id, date, jurisdiction_country_code, jurisdiction_region)
                    NULLS NOT DISTINCT;
                """);

            // Payroll credits trip loads and stops to the trip's drivers. Historical trips get their truck's
            // current drivers, the best record there is; drafts record them when dispatched.
            migrationBuilder.Sql("""
                UPDATE trips t
                SET driver_id = tr.main_driver_id, secondary_driver_id = tr.secondary_driver_id
                FROM trucks tr
                WHERE t.truck_id = tr.id
                  AND t.status <> 'draft';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS ux_customers_name_lower;
                DROP INDEX IF EXISTS ux_employees_email_lower;
                DROP INDEX IF EXISTS ux_trucks_number_lower;
                DROP INDEX IF EXISTS ux_truck_jurisdiction_mileage_truck_date_jurisdiction;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_agent_messages_agent_session_session_id",
                table: "agent_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_condition_defects_document_photo_document_id",
                table: "condition_defects");

            migrationBuilder.DropForeignKey(
                name: "fk_factoring_batch_items_payment_payment_id",
                table: "factoring_batch_items");

            migrationBuilder.DropForeignKey(
                name: "fk_fuel_card_transactions_expenses_expense_id",
                table: "fuel_card_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_load_accessorials_trip_stop_trip_stop_id",
                table: "load_accessorials");

            migrationBuilder.DropForeignKey(
                name: "fk_negotiation_messages_agent_decisions_agent_decision_id",
                table: "negotiation_messages");

            migrationBuilder.DropForeignKey(
                name: "fk_rate_negotiations_agent_conversations_conversation_id",
                table: "rate_negotiations");

            migrationBuilder.DropForeignKey(
                name: "fk_trips_employees_driver_id",
                table: "trips");

            migrationBuilder.DropForeignKey(
                name: "fk_trips_employees_secondary_driver_id",
                table: "trips");

            migrationBuilder.DropTable(
                name: "notification_reads");

            migrationBuilder.DropIndex(
                name: "ix_trucks_main_driver_id",
                table: "trucks");

            migrationBuilder.DropIndex(
                name: "ix_trucks_secondary_driver_id",
                table: "trucks");

            migrationBuilder.DropIndex(
                name: "ix_trucks_vin",
                table: "trucks");

            migrationBuilder.DropCheckConstraint(
                name: "ck_trucks_main_driver_not_secondary",
                table: "trucks");

            migrationBuilder.DropIndex(
                name: "ix_trips_driver_id",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_trips_secondary_driver_id",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_trips_status",
                table: "trips");

            migrationBuilder.DropIndex(
                name: "ix_trailers_vin",
                table: "trailers");

            migrationBuilder.DropIndex(
                name: "ix_rate_negotiations_conversation_id",
                table: "rate_negotiations");

            migrationBuilder.DropIndex(
                name: "ix_payments_stripe_payment_intent_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_stripe_transfer_id",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_notifications_created_date",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "ix_negotiation_messages_agent_decision_id",
                table: "negotiation_messages");

            migrationBuilder.DropIndex(
                name: "ix_loads_source_external_source_id",
                table: "loads");

            migrationBuilder.DropIndex(
                name: "ix_loads_status",
                table: "loads");

            migrationBuilder.DropIndex(
                name: "ix_load_accessorials_trip_stop_id",
                table: "load_accessorials");

            migrationBuilder.DropIndex(
                name: "ix_hos_violations_provider_type_external_violation_id",
                table: "hos_violations");

            migrationBuilder.DropIndex(
                name: "ix_hos_logs_employee_id_provider_type_external_log_id",
                table: "hos_logs");

            migrationBuilder.DropIndex(
                name: "ix_fuel_card_transactions_expense_id",
                table: "fuel_card_transactions");

            migrationBuilder.DropIndex(
                name: "ix_factoring_batch_items_payment_id",
                table: "factoring_batch_items");

            migrationBuilder.DropIndex(
                name: "ix_customer_users_user_id",
                table: "customer_users");

            migrationBuilder.DropIndex(
                name: "ix_condition_defects_photo_document_id",
                table: "condition_defects");

            migrationBuilder.DropIndex(
                name: "ix_agent_sessions_created_at",
                table: "agent_sessions");

            migrationBuilder.DropIndex(
                name: "ix_agent_messages_session_id",
                table: "agent_messages");

            migrationBuilder.DropColumn(
                name: "driver_id",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "secondary_driver_id",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "stripe_transfer_id",
                table: "payments");

            migrationBuilder.AlterColumn<string>(
                name: "vin",
                table: "trucks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(17)",
                oldMaxLength: 17,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "number",
                table: "trucks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                table: "tracking_links",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "first_name",
                table: "telegram_chats",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "group_title",
                table: "telegram_chats",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_interaction_at",
                table: "telegram_chats",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "telegram_chats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "telegram_chats",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "recorded_by_user_id",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "is_read",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "maintenance_schedule_id",
                table: "maintenance_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_broker_reference",
                table: "loads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_source_provider",
                table: "loads",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_hazmat",
                table: "loads",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE loads SET is_hazmat = true WHERE hazmat_class IS NOT NULL;");

            migrationBuilder.AddColumn<Guid>(
                name: "reported_by_id",
                table: "load_exceptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "raw_json",
                table: "load_board_listings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_account_id",
                table: "load_board_configurations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE invoices ALTER COLUMN period_start TYPE timestamp with time zone
                    USING (period_start::timestamp AT TIME ZONE 'UTC');
                ALTER TABLE invoices ALTER COLUMN period_end TYPE timestamp with time zone
                    USING (period_end::timestamp AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AddColumn<DateTime>(
                name: "billing_period_end",
                table: "invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "billing_period_start",
                table: "invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_invoice_id",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "subscription_id",
                table: "invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "tax_rate_percent",
                table: "invoice_line_items",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(7,4)",
                oldPrecision: 7,
                oldScale: 4);

            migrationBuilder.AddColumn<Guid>(
                name: "truck_expense_truck_id",
                table: "expenses",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE expenses SET truck_expense_truck_id = truck_id, truck_id = NULL WHERE type = 'truck';");

            migrationBuilder.Sql("""
                ALTER TABLE driver_licenses ALTER COLUMN issued_date TYPE timestamp with time zone
                    USING (issued_date::timestamp AT TIME ZONE 'UTC');
                ALTER TABLE driver_licenses ALTER COLUMN expires_at TYPE timestamp with time zone
                    USING (expires_at::timestamp AT TIME ZONE 'UTC');
                ALTER TABLE driver_licenses ALTER COLUMN medical_cert_expires_at TYPE timestamp with time zone
                    USING (medical_cert_expires_at::timestamp AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_reminder_sent_at",
                table: "driver_licenses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_driver_id",
                table: "driver_hos_statuses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_type",
                table: "driver_hos_statuses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "accident_report_id",
                table: "documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "dvir_report_id",
                table: "documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "maintenance_record_id",
                table: "documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "documents",
                type: "text",
                nullable: false,
                defaultValue: "active");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_login_at",
                table: "customer_users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "generation_cost_usd",
                table: "ai_dispatch_policies",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                table: "agent_sessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "summary",
                table: "agent_sessions",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "driver_behavior_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewed_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: true),
                    duration_seconds = table.Column<int>(type: "integer", nullable: true),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    external_event_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    g_force = table.Column<double>(type: "double precision", nullable: true),
                    is_dismissed = table.Column<bool>(type: "boolean", nullable: true),
                    is_reviewed = table.Column<bool>(type: "boolean", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    provider_type = table.Column<string>(type: "text", nullable: false),
                    raw_event_data_json = table.Column<string>(type: "text", nullable: true),
                    review_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    speed_limit_mph = table.Column<double>(type: "double precision", nullable: true),
                    speed_mph = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_driver_behavior_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_driver_behavior_events_employee_employee_id",
                        column: x => x.employee_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_driver_behavior_events_employee_reviewed_by_id",
                        column: x => x.reviewed_by_id,
                        principalTable: "employees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_driver_behavior_events_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    truck_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    days_interval = table.Column<int>(type: "integer", nullable: true),
                    engine_hours_interval = table.Column<int>(type: "integer", nullable: true),
                    interval_type = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_service_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_service_engine_hours = table.Column<int>(type: "integer", nullable: true),
                    last_service_mileage = table.Column<int>(type: "integer", nullable: true),
                    maintenance_type = table.Column<string>(type: "text", nullable: false),
                    mileage_interval = table.Column<int>(type: "integer", nullable: true),
                    next_due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_due_engine_hours = table.Column<int>(type: "integer", nullable: true),
                    next_due_mileage = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_maintenance_schedules", x => x.id);
                    table.ForeignKey(
                        name: "fk_maintenance_schedules_truck_truck_id",
                        column: x => x.truck_id,
                        principalTable: "trucks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trucks_main_driver_id",
                table: "trucks",
                column: "main_driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_trucks_number",
                table: "trucks",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trucks_secondary_driver_id",
                table: "trucks",
                column: "secondary_driver_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_records_maintenance_schedule_id",
                table: "maintenance_records",
                column: "maintenance_schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_hos_violations_external_violation_id",
                table: "hos_violations",
                column: "external_violation_id");

            migrationBuilder.CreateIndex(
                name: "ix_hos_logs_external_log_id",
                table: "hos_logs",
                column: "external_log_id");

            migrationBuilder.CreateIndex(
                name: "ix_expenses_truck_id1",
                table: "expenses",
                column: "truck_expense_truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_accident_report_id",
                table: "documents",
                column: "accident_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_dvir_report_id",
                table: "documents",
                column: "dvir_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_documents_maintenance_record_id",
                table: "documents",
                column: "maintenance_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_customer_users_user_id",
                table: "customer_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_agent_sessions_started_at",
                table: "agent_sessions",
                column: "started_at")
                .Annotation("Npgsql:IndexInclude", new[] { "estimated_cost_usd" });

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_employee_id_occurred_at",
                table: "driver_behavior_events",
                columns: new[] { "employee_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_event_type",
                table: "driver_behavior_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_external_event_id",
                table: "driver_behavior_events",
                column: "external_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_reviewed_by_id",
                table: "driver_behavior_events",
                column: "reviewed_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_driver_behavior_events_truck_id",
                table: "driver_behavior_events",
                column: "truck_id");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_is_active",
                table: "maintenance_schedules",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_next_due_date",
                table: "maintenance_schedules",
                column: "next_due_date");

            migrationBuilder.CreateIndex(
                name: "ix_maintenance_schedules_truck_id_maintenance_type",
                table: "maintenance_schedules",
                columns: new[] { "truck_id", "maintenance_type" });

            migrationBuilder.AddForeignKey(
                name: "fk_documents_accident_reports_accident_report_id",
                table: "documents",
                column: "accident_report_id",
                principalTable: "accident_reports",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_dvir_reports_dvir_report_id",
                table: "documents",
                column: "dvir_report_id",
                principalTable: "dvir_reports",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_documents_maintenance_records_maintenance_record_id",
                table: "documents",
                column: "maintenance_record_id",
                principalTable: "maintenance_records",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_expenses_trucks_truck_id1",
                table: "expenses",
                column: "truck_expense_truck_id",
                principalTable: "trucks",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_maintenance_records_maintenance_schedule_maintenance_schedu",
                table: "maintenance_records",
                column: "maintenance_schedule_id",
                principalTable: "maintenance_schedules",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <summary>
        /// A DO block that raises, listing every offender, when <paramref name="query"/> (one text column)
        /// returns any rows. <paramref name="message"/> must not contain quotes or percent signs.
        /// </summary>
        private static string FailIfAny(string message, string query) => $"""
            DO $$
            DECLARE offenders text;
            BEGIN
                SELECT string_agg(offender, '; ') INTO offenders FROM ({query}) AS q(offender);
                IF offenders IS NOT NULL THEN
                    RAISE EXCEPTION 'PilotDataIntegrity pre-flight: {message}: %', offenders;
                END IF;
            END $$;
            """;
    }
}
