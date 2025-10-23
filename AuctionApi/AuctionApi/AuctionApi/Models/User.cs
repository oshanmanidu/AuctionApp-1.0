using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace AuctionApi.Models
{
    //public class User
    //{
    //    public int Id { get; set; }
    //    [Required]

    //    //public string Username { get; set; } = string.Empty;
    //    public string Email { get; set; } = string.Empty; // ← Replaces Username

    //    [Required]
    //    public string PasswordHash { get; set; } = string.Empty;
    //    [Required]
    //    public string Role { get; set; } = "User"; // "User" or "Admin"
    //    public List<AuctionItem> AuctionItems { get; set; } = new();
    //    public List<Bid> Bids { get; set; } = new();
    //}

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