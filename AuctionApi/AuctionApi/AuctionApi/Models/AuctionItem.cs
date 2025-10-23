// Models/AuctionItem.cs
using System.ComponentModel.DataAnnotations;

namespace AuctionApi.Models
{
    public class AuctionItem
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;




        public string Description { get; set; } = string.Empty;

        public decimal StartingPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        


        // ✅ Bidding Window
        public DateTime? BidStartTime { get; set; }
        public DateTime? BidEndTime { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public List<Bid> Bids { get; set; } = new();
        public bool IsClosed { get; set; } = false;

        public string? WinnerEmail { get; set; }
        public decimal WinningBidAmount { get; set; }

        public string? ImageUrl { get; set; }

        // ✅ Computed Properties
        public bool IsBiddingOpen => BidStartTime.HasValue && BidEndTime.HasValue &&
                                     DateTime.UtcNow >= BidStartTime.Value &&
                                     DateTime.UtcNow <= BidEndTime.Value;

        public decimal CurrentHighestBid => Bids.Any() ? Bids.Max(b => b.Amount) : StartingPrice;
    }
}

