using AuctionApi.Data;
using AuctionApi.Hubs;
using AuctionApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuctionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class BidsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public BidsController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<ActionResult<Bid>> PlaceBid([FromBody] BidDto bidDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var item = await _context.AuctionItems
                .Include(a => a.Bids) // Include bids to calculate highest
                .FirstOrDefaultAsync(a => a.Id == bidDto.AuctionItemId);

            if (item == null)
                return NotFound("Auction item not found.");

            var highestBid = item.Bids.Any() ? item.Bids.Max(b => b.Amount) : item.StartingPrice;

            if (bidDto.Amount <= highestBid)
            {
                return BadRequest($"Bid must be higher than current highest bid: {highestBid:C}");
            }

            var bid = new Bid
            {
                Amount = bidDto.Amount,
                AuctionItemId = bidDto.AuctionItemId,
                UserId = userId,
                BidTime = DateTime.UtcNow
            };

            _context.Bids.Add(bid);
            await _context.SaveChangesAsync();

            // ✅ Reload item with new highest bid
            var updatedHighestBid = item.Bids
                .Concat(new[] { bid })
                .Max(b => b.Amount);

            // ✅ Get bidder name safely
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "A user";

            // ✅ Send real-time notification to all clients
            await _hubContext.Clients.All.SendAsync("ReceiveBid", new
            {
                auctionItemId = item.Id,
                highestBid = updatedHighestBid,
                bidderName = username,
                amount = bid.Amount,
                message = $"{username} placed a bid of ${bid.Amount} on {item.Name}"
            });

            // ✅ Return created bid
            return CreatedAtAction(nameof(GetBid), new { id = bid.Id }, bid);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Bid>> GetBid(int id)
        {
            var bid = await _context.Bids
                .Include(b => b.User)
                .Include(b => b.AuctionItem)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bid == null) return NotFound();
            return Ok(bid);
        }

        // ✅ DTO for creating bid
        public class BidDto
        {
            public int AuctionItemId { get; set; }
            public decimal Amount { get; set; }
        }
    }
}