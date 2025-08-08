using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddLoadDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoadDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BlobContainer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Active"),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedById = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadDocuments_Employees_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoadDocuments_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadDocuments_CreatedAt",
                table: "LoadDocuments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoadDocuments_LoadId",
                table: "LoadDocuments",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadDocuments_LoadId_Status_Type",
                table: "LoadDocuments",
                columns: new[] { "LoadId", "Status", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_LoadDocuments_Status",
                table: "LoadDocuments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LoadDocuments_Type",
                table: "LoadDocuments",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_LoadDocuments_UploadedById",
                table: "LoadDocuments",
                column: "UploadedById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoadDocuments");
        }
    }
}
