using System.ComponentModel.DataAnnotations;

namespace AuctionApi.Models
{
    public class Bid
    {
        public int Id { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public DateTime BidTime { get; set; } = DateTime.UtcNow;
        public int AuctionItemId { get; set; }
        public AuctionItem AuctionItem { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}