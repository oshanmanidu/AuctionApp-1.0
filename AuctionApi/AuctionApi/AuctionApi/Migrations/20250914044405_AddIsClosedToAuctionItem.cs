using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIsClosedToAuctionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "AuctionItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "AuctionItems");
        }
    }
}
