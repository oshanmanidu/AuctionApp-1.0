using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace AuctionApi.Models
{
    

    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public List<AuctionItem> AuctionItems { get; set; } = new();
        public List<Bid> Bids { get; set; } = new();
    }
}