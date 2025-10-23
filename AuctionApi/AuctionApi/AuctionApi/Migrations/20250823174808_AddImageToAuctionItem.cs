using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuctionApi.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToAuctionItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "AuctionItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "AuctionItems");
        }
    }
}
