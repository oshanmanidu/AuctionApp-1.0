using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddBidTiming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BidEndTime",
                table: "AuctionItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BidStartTime",
                table: "AuctionItems",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BidEndTime",
                table: "AuctionItems");

            migrationBuilder.DropColumn(
                name: "BidStartTime",
                table: "AuctionItems");
        }
    }
}
