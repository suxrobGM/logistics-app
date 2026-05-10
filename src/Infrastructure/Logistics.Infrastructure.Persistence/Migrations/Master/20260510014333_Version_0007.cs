using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class Version_0007 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete anonymous-only consent rows. We no longer record consent for
            // unauthenticated visitors, and these rows cannot satisfy the new
            // non-null UserId + Cascade FK to users.
            migrationBuilder.Sql("DELETE FROM consent_records WHERE user_id IS NULL;");

            migrationBuilder.DropForeignKey(
                name: "fk_consent_records_users_user_id",
                table: "consent_records");

            migrationBuilder.DropIndex(
                name: "ix_consent_records_anonymous_id",
                table: "consent_records");

            migrationBuilder.DropColumn(
                name: "anonymous_id",
                table: "consent_records");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "consent_records",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_consent_records_users_user_id",
                table: "consent_records",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_consent_records_users_user_id",
                table: "consent_records");

            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "consent_records",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "anonymous_id",
                table: "consent_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_consent_records_anonymous_id",
                table: "consent_records",
                column: "anonymous_id");

            migrationBuilder.AddForeignKey(
                name: "fk_consent_records_users_user_id",
                table: "consent_records",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
