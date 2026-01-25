using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Data.Migrations.Master
{
    /// <inheritdoc />
    public partial class Version_0002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "Invoices",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedById",
                table: "Invoices",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Invoices",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalDistanceDriven",
                table: "Invoices",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHoursWorked",
                table: "Invoices",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ApprovedById",
                table: "Invoices",
                column: "ApprovedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_AspNetUsers_ApprovedById",
                table: "Invoices",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_AspNetUsers_ApprovedById",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ApprovedById",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TotalDistanceDriven",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TotalHoursWorked",
                table: "Invoices");
        }
    }
}
