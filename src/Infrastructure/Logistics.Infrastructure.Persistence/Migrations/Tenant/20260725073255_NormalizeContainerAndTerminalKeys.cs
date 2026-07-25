using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <summary>
    /// Uppercases <c>containers.number</c> (ISO 6346) and <c>terminals.code</c> (UN/LOCODE) to match
    /// the entities, which now canonicalise on write. Their unique indexes are case-sensitive
    /// B-trees, so "mscu1234567" and "MSCU1234567" were two rows and lookups picked one arbitrarily.
    /// Data-only, so there is no model snapshot diff.
    /// </summary>
    public partial class NormalizeContainerAndTerminalKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Folding case-only duplicates together violates the unique index, and there is no safe
            // automatic winner for business data - so name them and stop.
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    conflicting text;
                BEGIN
                    SELECT string_agg(k, ', ') INTO conflicting FROM (
                        SELECT upper(btrim(number)) AS k
                        FROM containers
                        GROUP BY upper(btrim(number))
                        HAVING count(*) > 1
                    ) d;

                    IF conflicting IS NOT NULL THEN
                        RAISE EXCEPTION
                            'Cannot normalise containers.number: these values differ only by case or whitespace and would collide: %. Merge or rename them, then re-run this migration.',
                            conflicting;
                    END IF;

                    UPDATE containers
                    SET number = upper(btrim(number))
                    WHERE number <> upper(btrim(number));
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    conflicting text;
                BEGIN
                    SELECT string_agg(k, ', ') INTO conflicting FROM (
                        SELECT upper(btrim(code)) AS k
                        FROM terminals
                        GROUP BY upper(btrim(code))
                        HAVING count(*) > 1
                    ) d;

                    IF conflicting IS NOT NULL THEN
                        RAISE EXCEPTION
                            'Cannot normalise terminals.code: these values differ only by case or whitespace and would collide: %. Merge or rename them, then re-run this migration.',
                            conflicting;
                    END IF;

                    UPDATE terminals
                    SET code = upper(btrim(code))
                    WHERE code <> upper(btrim(code));
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The original casing is recorded nowhere, so a no-op is honest and a revert would lie.
        }
    }
}
