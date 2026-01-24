using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TruckId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TruckId",
                table: "Documents",
                column: "TruckId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_Trucks_TruckId",
                table: "Documents",
                column: "TruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_Trucks_TruckId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_TruckId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "TruckId",
                table: "Documents");
        }
    }
}
