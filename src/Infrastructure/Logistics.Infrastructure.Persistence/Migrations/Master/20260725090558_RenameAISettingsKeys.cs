using Logistics.Application.Abstractions.AI;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <summary>
    /// Data-only: renames the two AI dispatch <c>system_settings</c> keys to the casing
    /// <c>AISettingsKeys</c> now uses. Postgres collation is case-sensitive, so without this the old
    /// rows go unreachable and the admin's saved model choice silently reverts to the appsettings
    /// default. The NOT EXISTS guards keep the unique index on <c>key</c> safe if both forms exist.
    /// </summary>
    public partial class RenameAISettingsKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE system_settings SET key = 'AI.Model'
                WHERE key = 'Ai.Model'
                  AND NOT EXISTS (SELECT 1 FROM system_settings WHERE key = 'AI.Model');
                """);

            migrationBuilder.Sql(
                """
                UPDATE system_settings SET key = 'AI.ExtendedThinking'
                WHERE key = 'Ai.ExtendedThinking'
                  AND NOT EXISTS (SELECT 1 FROM system_settings WHERE key = 'AI.ExtendedThinking');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE system_settings SET key = 'Ai.Model'
                WHERE key = 'AI.Model'
                  AND NOT EXISTS (SELECT 1 FROM system_settings WHERE key = 'Ai.Model');
                """);

            migrationBuilder.Sql(
                """
                UPDATE system_settings SET key = 'Ai.ExtendedThinking'
                WHERE key = 'AI.ExtendedThinking'
                  AND NOT EXISTS (SELECT 1 FROM system_settings WHERE key = 'Ai.ExtendedThinking');
                """);
        }
    }
}
