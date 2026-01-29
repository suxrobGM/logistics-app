using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Persistence.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0003 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_DriverCertifications_DriverCertificationId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_TrainingRecords_TrainingRecordId",
                table: "Documents");

            migrationBuilder.DropTable(
                name: "DriverCertifications");

            migrationBuilder.DropTable(
                name: "TrainingRecords");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DriverCertificationId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_TrainingRecordId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DriverCertificationId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "TrainingRecordId",
                table: "Documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DriverCertificationId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingRecordId",
                table: "Documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverCertifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    VerifiedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CdlClass = table.Column<int>(type: "integer", nullable: true),
                    CertificationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CertificationType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Endorsements = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssuingAuthority = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IssuingState = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Restrictions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverCertifications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverCertifications_Employees_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrainingRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Hours = table.Column<decimal>(type: "numeric", nullable: true),
                    IsPassed = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Provider = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Score = table.Column<decimal>(type: "numeric", nullable: true),
                    TrainingName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TrainingType = table.Column<int>(type: "integer", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainingRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DriverCertificationId",
                table: "Documents",
                column: "DriverCertificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_TrainingRecordId",
                table: "Documents",
                column: "TrainingRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertifications_EmployeeId_CertificationType",
                table: "DriverCertifications",
                columns: new[] { "EmployeeId", "CertificationType" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertifications_ExpirationDate",
                table: "DriverCertifications",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertifications_Status",
                table: "DriverCertifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DriverCertifications_VerifiedById",
                table: "DriverCertifications",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecords_CompletedDate",
                table: "TrainingRecords",
                column: "CompletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingRecords_EmployeeId_TrainingType",
                table: "TrainingRecords",
                columns: new[] { "EmployeeId", "TrainingType" });

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_DriverCertifications_DriverCertificationId",
                table: "Documents",
                column: "DriverCertificationId",
                principalTable: "DriverCertifications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_TrainingRecords_TrainingRecordId",
                table: "Documents",
                column: "TrainingRecordId",
                principalTable: "TrainingRecords",
                principalColumn: "Id");
        }
    }
}
