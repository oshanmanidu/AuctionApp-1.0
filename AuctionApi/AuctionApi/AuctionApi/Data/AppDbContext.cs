using Microsoft.EntityFrameworkCore;
using AuctionApi.Models;

namespace AuctionApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<AuctionItem> AuctionItems { get; set; }
        public DbSet<Bid> Bids { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fix decimal precision
            modelBuilder.Entity<AuctionItem>()
                .Property(a => a.StartingPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Bid>()
                .Property(b => b.Amount)
                .HasPrecision(18, 2);

            // ❌ Break the multiple cascade path: User → Bids
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bids)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict); // 👈 Prevent cascade from User to Bid

            // Optional: Explicitly define other relationships (usually not needed)
            modelBuilder.Entity<AuctionItem>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuctionItems)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.AuctionItem)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}