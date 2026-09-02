using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddProductLicenseHeartbeats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_license_heartbeats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    instance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hostname = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    key_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    licensee = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    tenant_count = table.Column<int>(type: "integer", nullable: false),
                    first_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_license_heartbeats", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_license_heartbeats_instance_id",
                table: "product_license_heartbeats",
                column: "instance_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_license_heartbeats_last_seen_at",
                table: "product_license_heartbeats",
                column: "last_seen_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_license_heartbeats");
        }
    }
}
