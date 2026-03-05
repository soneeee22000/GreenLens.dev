using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenLens.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarbonEstimates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalCo2eKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarbonEstimates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResourceUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CarbonEstimateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResourceType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Hours = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Region = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Co2eKg = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Co2ePerUnit = table.Column<decimal>(type: "TEXT", precision: 18, scale: 8, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceUsages_CarbonEstimates_CarbonEstimateId",
                        column: x => x.CarbonEstimateId,
                        principalTable: "CarbonEstimates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarbonEstimates_CreatedAt",
                table: "CarbonEstimates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceUsages_CarbonEstimateId",
                table: "ResourceUsages",
                column: "CarbonEstimateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResourceUsages");

            migrationBuilder.DropTable(
                name: "CarbonEstimates");
        }
    }
}
