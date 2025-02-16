using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.EF.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: true),
                    PaymentFor = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    BillingAddress_City = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_Country = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    BillingAddress_Region = table.Column<string>(type: "text", nullable: false),
                    BillingAddress_ZipCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    NormalizedName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trucks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    TruckNumber = table.Column<string>(type: "text", nullable: false),
                    CurrentLocationLat = table.Column<double>(type: "double precision", nullable: true),
                    CurrentLocationLong = table.Column<double>(type: "double precision", nullable: true),
                    CurrentLocation_City = table.Column<string>(type: "text", nullable: false),
                    CurrentLocation_Country = table.Column<string>(type: "text", nullable: false),
                    CurrentLocation_Line1 = table.Column<string>(type: "text", nullable: false),
                    CurrentLocation_Line2 = table.Column<string>(type: "text", nullable: true),
                    CurrentLocation_Region = table.Column<string>(type: "text", nullable: false),
                    CurrentLocation_ZipCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trucks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LoadId = table.Column<string>(type: "text", nullable: false),
                    CustomerId = table.Column<string>(type: "text", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: false),
                    ClaimValue = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Salary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SalaryType = table.Column<int>(type: "integer", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceToken = table.Column<string>(type: "text", nullable: true),
                    TruckId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRoles",
                columns: table => new
                {
                    EmployeeId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeRoles", x => new { x.EmployeeId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_EmployeeRoles_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Loads",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    RefId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    OriginAddressLat = table.Column<double>(type: "double precision", nullable: true),
                    OriginAddressLong = table.Column<double>(type: "double precision", nullable: true),
                    DestinationAddressLat = table.Column<double>(type: "double precision", nullable: true),
                    DestinationAddressLong = table.Column<double>(type: "double precision", nullable: true),
                    DeliveryCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Distance = table.Column<double>(type: "double precision", nullable: false),
                    CanConfirmPickUp = table.Column<bool>(type: "boolean", nullable: false),
                    CanConfirmDelivery = table.Column<bool>(type: "boolean", nullable: false),
                    DispatchedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PickUpDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CustomerId = table.Column<string>(type: "text", nullable: true),
                    InvoiceId = table.Column<string>(type: "text", nullable: true),
                    AssignedTruckId = table.Column<string>(type: "text", nullable: true),
                    AssignedDispatcherId = table.Column<string>(type: "text", nullable: true),
                    DestinationAddress_City = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Country = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    DestinationAddress_Region = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_City = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Country = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    OriginAddress_Region = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_ZipCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loads_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Loads_Employees_AssignedDispatcherId",
                        column: x => x.AssignedDispatcherId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Loads_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Loads_Trucks_AssignedTruckId",
                        column: x => x.AssignedTruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Payrolls",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentId = table.Column<string>(type: "text", nullable: false),
                    EmployeeId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payrolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payrolls_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payrolls_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRoles_RoleId",
                table: "EmployeeRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_TruckId",
                table: "Employees",
                column: "TruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PaymentId",
                table: "Invoices",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loads_AssignedDispatcherId",
                table: "Loads",
                column: "AssignedDispatcherId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_AssignedTruckId",
                table: "Loads",
                column: "AssignedTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_CustomerId",
                table: "Loads",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Loads_InvoiceId",
                table: "Loads",
                column: "InvoiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Loads_RefId",
                table: "Loads",
                column: "RefId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_EmployeeId",
                table: "Payrolls",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_PaymentId",
                table: "Payrolls",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeRoles");

            migrationBuilder.DropTable(
                name: "Loads");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payrolls");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Trucks");
        }
    }
}
