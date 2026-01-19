using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistics.Infrastructure.Data.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class Version_0002 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalBrokerReference",
                table: "Loads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalSourceId",
                table: "Loads",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalSourceProvider",
                table: "Loads",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoadBoardConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApiSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AccessToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RefreshToken = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WebhookSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExternalAccountId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CompanyDotNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CompanyMcNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadBoardConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadBoardListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalListingId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    RatePerMile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Distance = table.Column<double>(type: "double precision", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: true),
                    Length = table.Column<int>(type: "integer", nullable: true),
                    PickupDateStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PickupDateEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDateStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryDateEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EquipmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Commodity = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BrokerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BrokerPhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BrokerEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BrokerMcNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BookedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoadId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RawJson = table.Column<string>(type: "text", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DestinationAddress_City = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Country = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    DestinationAddress_State = table.Column<string>(type: "text", nullable: false),
                    DestinationAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    DestinationLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    DestinationLocation_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    OriginAddress_City = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Country = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    OriginAddress_State = table.Column<string>(type: "text", nullable: false),
                    OriginAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    OriginLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    OriginLocation_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    TotalRate_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalRate_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadBoardListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadBoardListings_Loads_LoadId",
                        column: x => x.LoadId,
                        principalTable: "Loads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PostedTrucks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TruckId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderType = table.Column<int>(type: "integer", nullable: false),
                    ExternalPostId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DestinationRadius = table.Column<int>(type: "integer", nullable: true),
                    AvailableFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AvailableTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EquipmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaxWeight = table.Column<int>(type: "integer", nullable: true),
                    MaxLength = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRefreshedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AvailableAtAddress_City = table.Column<string>(type: "text", nullable: false),
                    AvailableAtAddress_Country = table.Column<string>(type: "text", nullable: false),
                    AvailableAtAddress_Line1 = table.Column<string>(type: "text", nullable: false),
                    AvailableAtAddress_Line2 = table.Column<string>(type: "text", nullable: true),
                    AvailableAtAddress_State = table.Column<string>(type: "text", nullable: false),
                    AvailableAtAddress_ZipCode = table.Column<string>(type: "text", nullable: false),
                    AvailableAtLocation_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    AvailableAtLocation_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    DestinationPreference_City = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_Country = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_Line1 = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_Line2 = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_State = table.Column<string>(type: "text", nullable: true),
                    DestinationPreference_ZipCode = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostedTrucks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostedTrucks_Trucks_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Trucks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardConfigurations_ProviderType",
                table: "LoadBoardConfigurations",
                column: "ProviderType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardListings_ExpiresAt",
                table: "LoadBoardListings",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardListings_ExternalListingId_ProviderType",
                table: "LoadBoardListings",
                columns: new[] { "ExternalListingId", "ProviderType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardListings_LoadId",
                table: "LoadBoardListings",
                column: "LoadId");

            migrationBuilder.CreateIndex(
                name: "IX_LoadBoardListings_Status",
                table: "LoadBoardListings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PostedTrucks_ExternalPostId",
                table: "PostedTrucks",
                column: "ExternalPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostedTrucks_Status",
                table: "PostedTrucks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PostedTrucks_TruckId_ProviderType",
                table: "PostedTrucks",
                columns: new[] { "TruckId", "ProviderType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoadBoardConfigurations");

            migrationBuilder.DropTable(
                name: "LoadBoardListings");

            migrationBuilder.DropTable(
                name: "PostedTrucks");

            migrationBuilder.DropColumn(
                name: "ExternalBrokerReference",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "ExternalSourceId",
                table: "Loads");

            migrationBuilder.DropColumn(
                name: "ExternalSourceProvider",
                table: "Loads");
        }
    }
}
