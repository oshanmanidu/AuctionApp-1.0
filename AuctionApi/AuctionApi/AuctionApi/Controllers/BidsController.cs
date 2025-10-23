//using AuctionApi.Data;
//using AuctionApi.Hubs;
//using AuctionApi.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using System.Security.Claims;

//namespace AuctionApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize(Roles = "User")]
//    public class BidsController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly IHubContext<NotificationHub> _hubContext; // ← Inject SignalR

//        public BidsController(AppDbContext context, IHubContext<NotificationHub> hubContext)
//        {
//            _context = context;
//            _hubContext = hubContext; // ← Store reference
//        }

//        [HttpPost]
//        //public async Task<ActionResult<Bid>> PlaceBid([FromBody] Bid bid)
//        //{
//        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        //    if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
//        //        return Unauthorized();

//        //    var user = await _context.Users.FindAsync(userId);
//        //    if (user == null) return NotFound("User not found");

//        //    var item = await _context.AuctionItems.FindAsync(bid.AuctionItemId);
//        //    if (item == null) return NotFound("Auction item not found");

//        //    // Optional: Validate bid amount > current highest bid
//        //    var highestBid = await _context.Bids
//        //        .Where(b => b.AuctionItemId == bid.AuctionItemId)
//        //        .Select(b => b.Amount)
//        //        .DefaultIfEmpty(item.StartingPrice)
//        //        .MaxAsync();

//        //    if (bid.Amount <= highestBid)
//        //    {
//        //        return BadRequest($"Bid must be higher than current highest bid: {highestBid:C}");
//        //    }

//        //    bid.UserId = userId;
//        //    bid.BidTime = DateTime.UtcNow;

//        //    _context.Bids.Add(bid);
//        //    await _context.SaveChangesAsync();

//        //    // ✅ Send Real-Time Notification via SignalR
//        //    await _hubContext.Clients.All.SendAsync("ReceiveBid",
//        //        $"{user.Username} placed a bid of ${bid.Amount} on {item.Name}");

//        //    return CreatedAtAction(nameof(GetBid), new { id = bid.Id }, bid);
//        //}

//        [HttpPost]
//        public async Task<ActionResult<Bid>> PlaceBid([FromBody] Bid bid)
//        {
//            if (bid.AuctionItemId <= 0)
//                return BadRequest("Auction item ID is required.");

//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
//                return Unauthorized();

//            var item = await _context.AuctionItems.FindAsync(bid.AuctionItemId);
//            if (item == null)
//                return NotFound("Auction item not found.");

//            var highestBid = await _context.Bids
//                .Where(b => b.AuctionItemId == bid.AuctionItemId)
//                .Select(b => b.Amount)
//                .DefaultIfEmpty(item.StartingPrice)
//                .MaxAsync();

//            if (bid.Amount <= highestBid)
//            {
//                return BadRequest($"Bid must be higher than current highest bid: {highestBid:C}");
//            }

//            bid.UserId = userId;
//            bid.BidTime = DateTime.UtcNow;

//            _context.Bids.Add(bid);
//            await _context.SaveChangesAsync();

//            // SignalR notification
//            await _hubContext.Clients.All.SendAsync("ReceiveBid",
//                $"{User.FindFirst(ClaimTypes.Name)?.Value} placed a bid of ${bid.Amount} on {item.Name}");

//            return CreatedAtAction(nameof(GetBid), new { id = bid.Id }, bid);
//        }



//        [HttpGet("{id}")]
//        public async Task<ActionResult<Bid>> GetBid(int id)
//        {
//            var bid = await _context.Bids
//                .Include(b => b.User)
//                .Include(b => b.AuctionItem)
//                .FirstOrDefaultAsync(b => b.Id == id);

//            if (bid == null) return NotFound();
//            return bid;
//        }
//    }
//}

//using AuctionApi.Data;
//using AuctionApi.Hubs;
//using AuctionApi.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace AuctionApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize(Roles = "User")]
//    public class BidsController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly IHubContext<NotificationHub> _hubContext;

//        public BidsController(AppDbContext context, IHubContext<NotificationHub> hubContext)
//        {
//            _context = context;
//            _hubContext = hubContext;
//        }

//        [HttpPost]
//        public async Task<ActionResult<Bid>> PlaceBid([FromBody] BidDto bidDto)
//        {
//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
//                return Unauthorized();

//            var item = await _context.AuctionItems.FindAsync(bidDto.AuctionItemId);
//            if (item == null)
//                return NotFound("Auction item not found.");

//            // ✅ Force client-side evaluation
//            var bids = await _context.Bids
//                .Where(b => b.AuctionItemId == bidDto.AuctionItemId)
//                .Select(b => b.Amount)
//                .ToListAsync();

//            var highestBid = bids.Any() ? bids.Max() : item.StartingPrice;

//            if (bidDto.Amount <= highestBid)
//            {
//                return BadRequest($"Bid must be higher than current highest bid: {highestBid:C}");
//            }

//            var bid = new Bid
//            {
//                Amount = bidDto.Amount,
//                AuctionItemId = bidDto.AuctionItemId,
//                UserId = userId,
//                BidTime = DateTime.UtcNow
//            };

//            _context.Bids.Add(bid);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetBid), new { id = bid.Id }, bid);
//        }

//        [HttpGet("{id}")]
//        public async Task<ActionResult<Bid>> GetBid(int id)
//        {
//            var bid = await _context.Bids
//                .Include(b => b.User)
//                .Include(b => b.AuctionItem)
//                .FirstOrDefaultAsync(b => b.Id == id);

//            if (bid == null) return NotFound();
//            return Ok(bid);
//        }
//    }
//}



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