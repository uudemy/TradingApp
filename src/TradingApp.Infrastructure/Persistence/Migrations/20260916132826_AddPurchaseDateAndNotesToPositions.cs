using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseDateAndNotesToPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "portfolio_positions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PurchaseDate",
                table: "portfolio_positions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "portfolio_positions");

            migrationBuilder.DropColumn(
                name: "PurchaseDate",
                table: "portfolio_positions");
        }
    }
}
