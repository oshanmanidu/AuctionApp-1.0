using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionApi.Migrations
{
    /// <inheritdoc />
    public partial class fixs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WinnerEmail",
                table: "AuctionItems");

            migrationBuilder.DropColumn(
                name: "WinningBidAmount",
                table: "AuctionItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WinnerEmail",
                table: "AuctionItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WinningBidAmount",
                table: "AuctionItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
