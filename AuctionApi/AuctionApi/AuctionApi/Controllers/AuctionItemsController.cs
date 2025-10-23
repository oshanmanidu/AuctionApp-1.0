//using AuctionApi.Data;
//using AuctionApi.Hubs;
//using AuctionApi.Models;
//using AuctionApi.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using System.IO;
//using System.Security.Claims;

//namespace AuctionApi.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize(Roles = "User,Admin")]
//    public class AuctionItemsController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly IHubContext<NotificationHub> _hubContext;

//        public AuctionItemsController(AppDbContext context, IHubContext<NotificationHub> hubContext)
//        {
//            _context = context;
//            _hubContext = hubContext;
//        }

//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<object>>> GetItems()
//        {
//            try
//            {
//                var items = await _context.AuctionItems
//                    .Include(a => a.User)
//                    .Include(a => a.Bids) // ✅ Critical: Load bids
//                    .ThenInclude(b => b.User)
//                    .ToListAsync();

//                var result = items.Select(a => new
//                {
//                    a.Id,
//                    a.Name,
//                    a.Description,
//                    a.StartingPrice,
//                    a.CreatedAt,
//                    a.ImageUrl,
//                    a.BidStartTime,
//                    a.BidEndTime,
//                    a.CurrentHighestBid, // ✅ Now accurate
//                    IsBiddingOpen = a.IsBiddingOpen,
//                    User = new
//                    {
//                        a.User.Id,
//                        a.User.Email
//                    }
//                }).ToList();

//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error in GetItems: {ex.Message}");
//                return StatusCode(500, new { error = "Failed to load auction items." });
//            }
//        }

//        [HttpGet("{id}")]
//        public async Task<ActionResult> GetItem(int id)
//        {
//            try
//            {
//                var item = await _context.AuctionItems
//                    .Include(a => a.User)
//                    .Include(a => a.Bids)
//                        .ThenInclude(b => b.User)
//                    .FirstOrDefaultAsync(a => a.Id == id);

//                if (item == null) return NotFound(new { message = "Auction item not found." });

//                var result = new
//                {
//                    item.Id,
//                    item.Name,
//                    item.Description,
//                    item.StartingPrice,
//                    item.CreatedAt,
//                    item.ImageUrl,
//                    item.BidStartTime,
//                    item.BidEndTime,
//                    item.CurrentHighestBid,
//                    IsBiddingOpen = item.IsBiddingOpen,
//                    User = new
//                    {
//                        item.User.Id,
//                        item.User.Email
//                    },
//                    Bids = item.Bids.Select(b => new
//                    {
//                        b.Id,
//                        b.Amount,
//                        b.BidTime,
//                        User = new
//                        {
//                            b.User.Id,
//                            b.User.Email
//                        }
//                    }).ToList()
//                };

//                return Ok(result);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error in GetItem: {ex.Message}");
//                return StatusCode(500, new { error = "Failed to load auction item." });
//            }
//        }


//        [HttpPost]
//        public async Task<ActionResult> CreateItem([FromForm] AuctionItemDto model)
//        {
//            if (string.IsNullOrEmpty(model.Name) || model.StartingPrice <= 0)
//            {
//                return BadRequest(new { error = "Name and StartingPrice are required." });
//            }

//            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
//                return Unauthorized();

//            string imageUrl = null;

//            if (model.ImageFile != null && model.ImageFile.Length > 0)
//            {
//                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
//                if (!Directory.Exists(uploadsFolder))
//                    Directory.CreateDirectory(uploadsFolder);

//                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
//                var filePath = Path.Combine(uploadsFolder, fileName);

//                using (var stream = new FileStream(filePath, FileMode.Create))
//                {
//                    await model.ImageFile.CopyToAsync(stream);
//                }

//                imageUrl = $"/images/{fileName}";
//            }

//            var item = new AuctionItem
//            {
//                Name = model.Name,
//                Description = model.Description,
//                StartingPrice = model.StartingPrice,
//                UserId = userId,
//                ImageUrl = imageUrl,
//                CreatedAt = DateTime.UtcNow
//            };

//            _context.AuctionItems.Add(item);
//            await _context.SaveChangesAsync();

//            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, new
//            {
//                item.Id,
//                item.Name,
//                item.Description,
//                item.StartingPrice,
//                item.CreatedAt,
//                item.ImageUrl,
//                item.BidStartTime,
//                item.BidEndTime,
//                UserId = item.UserId,
//                User = new { item.User?.Id, item.User?.Email }
//            });
//        }

//        // PUT: api/auctionitems/start-bid/5
//        [HttpPut("start-bid/{id}")]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> StartBid(int id, [FromBody] StartBidModel model)
//        {
//            var item = await _context.AuctionItems.FindAsync(id);
//            if (item == null) return NotFound("Auction item not found.");

//            if (model.EndTime <= model.StartTime)
//                return BadRequest("End time must be after start time.");

//            item.BidStartTime = model.StartTime;
//            item.BidEndTime = model.EndTime;

//            await _context.SaveChangesAsync();

//            await _hubContext.Clients.All.SendAsync("ReceiveBid",
//                $"🎉 Bidding started for '{item.Name}'! Ends at {item.BidEndTime:HH:mm:ss}");

//            return Ok(new
//            {
//                message = "Bidding started successfully.",
//                item.BidStartTime,
//                item.BidEndTime,
//                IsBiddingOpen = item.IsBiddingOpen
//            });
//        }

//        // DELETE: api/auctionitems/5
//        [HttpDelete("{id}")]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> DeleteItem(int id)
//        {
//            var item = await _context.AuctionItems.FindAsync(id);
//            if (item == null) return NotFound();

//            if (!string.IsNullOrEmpty(item.ImageUrl))
//            {
//                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", item.ImageUrl.TrimStart('/'));
//                if (System.IO.File.Exists(imagePath))
//                {
//                    try
//                    {
//                        System.IO.File.Delete(imagePath);
//                    }
//                    catch (Exception ex)
//                    {
//                        Console.WriteLine($"Failed to delete image: {ex.Message}");
//                    }
//                }
//            }

//            _context.AuctionItems.Remove(item);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }

//        [HttpPut("end-auction/{id}")]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> EndAuction(int id, [FromServices] EmailService emailService)
//        {
//            var item = await _context.AuctionItems
//                .Include(a => a.Bids)
//                    .ThenInclude(b => b.User)
//                .FirstOrDefaultAsync(a => a.Id == id);

//            if (item == null) return NotFound("Auction item not found.");

//            if (DateTime.UtcNow < (item.BidStartTime ?? DateTime.MinValue))
//                return BadRequest("Auction hasn't started yet.");

//            if (!item.IsBiddingOpen && item.BidEndTime < DateTime.UtcNow)
//                return BadRequest("Auction already ended.");

//            // 🔍 Find winner (highest bidder)
//            var highestBid = item.Bids
//                .OrderByDescending(b => b.Amount)
//                .FirstOrDefault();

//            if (highestBid == null)
//                return BadRequest("No bids were placed.");

//            var winner = await _context.Users.FindAsync(highestBid.UserId);
//            var allBidders = item.Bids.Select(b => b.User).Distinct().ToList();

//            // 📧 Send emails
//            foreach (var user in allBidders)
//            {
//                try
//                {
//                    if (user.Id == winner.Id)
//                    {
//                        await emailService.SendEmailAsync(
//                            user.Email,
//                            $"🎉 Congratulations! You won '{item.Name}'",
//                            $@"
//                    <h2>Congratulations, {user.Email}!</h2>
//                    <p>You have won the auction for <strong>{item.Name}</strong> with a bid of <strong>${highestBid.Amount:F2}</strong>.</p>
//                    <p>Contact the admin to complete the payment.</p>
//                    <p><em>Thank you for using BiddingBoom!</em></p>"
//                        );
//                    }
//                    else
//                    {
//                        await emailService.SendEmailAsync(
//                            user.Email,
//                            $"🔚 Auction for '{item.Name}' has ended",
//                            $@"
//                    <h2>Hi there,</h2>
//                    <p>The auction for <strong>{item.Name}</strong> has ended.</p>
//                    <p>The winning bid was <strong>${highestBid.Amount:F2}</strong>.</p>
//                    <p>Better luck next time!</p>
//                    <p><em>Thanks for participating in BiddingBoom!</em></p>"
//                        );
//                    }
//                }
//                catch (Exception ex)
//                {
//                    // Log failed email (optional)
//                    Console.WriteLine($"Failed to send email to {user.Email}: {ex.Message}");
//                }
//            }

//            // Optional: Mark as closed in DB
//            // item.BidEndTime = DateTime.UtcNow;
//            // await _context.SaveChangesAsync();

//            return Ok(new
//            {
//                message = "Auction ended. Emails sent to all bidders.",
//                winner = new { winner.Email, Amount = highestBid.Amount }
//            });
//        }
//        // DTOs
//        public class AuctionItemDto
//        {
//            public string Name { get; set; } = string.Empty;
//            public string Description { get; set; } = string.Empty;
//            public decimal StartingPrice { get; set; }
//            public IFormFile? ImageFile { get; set; }
//        }

//        public class StartBidModel
//        {
//            public DateTime StartTime { get; set; }
//            public DateTime EndTime { get; set; }
//        }
//    }
//}

using AuctionApi.Data;
using AuctionApi.Hubs;
using AuctionApi.Models;
using AuctionApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Security.Claims;

namespace AuctionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User,Admin")]
    public class AuctionItemsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AuctionItemsController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/auctionitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetItems()
        {
            try
            {
                var items = await _context.AuctionItems
                    .Include(a => a.User)
                    .Include(a => a.Bids).ThenInclude(b => b.User)
                    .ToListAsync();

                var result = items.Select(a => new
                {
                    a.Id,
                    a.Name,
                    a.Description,
                    a.StartingPrice,
                    a.CreatedAt,
                    a.ImageUrl,
                    a.BidStartTime,
                    a.BidEndTime,
                    a.CurrentHighestBid,
                    IsBiddingOpen = a.IsBiddingOpen,
                    a.IsClosed,
                    a.WinnerEmail,
                    a.WinningBidAmount,
                    User = new
                    {
                        a.User.Id,
                        a.User.Email
                    }
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetItems: {ex.Message}");
                return StatusCode(500, new { error = "Failed to load auction items." });
            }
        }

        // GET: api/auctionitems/1
        [HttpGet("{id}")]
        public async Task<ActionResult> GetItem(int id)
        {
            try
            {
                var item = await _context.AuctionItems
                    .Include(a => a.User)
                    .Include(a => a.Bids).ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (item == null) return NotFound(new { message = "Auction item not found." });

                var result = new
                {
                    item.Id,
                    item.Name,
                    item.Description,
                    item.StartingPrice,
                    item.CreatedAt,
                    item.ImageUrl,
                    item.BidStartTime,
                    item.BidEndTime,
                    item.CurrentHighestBid,
                    IsBiddingOpen = item.IsBiddingOpen,
                    item.IsClosed,
                    item.WinnerEmail,
                    item.WinningBidAmount,
                    User = new
                    {
                        item.User.Id,
                        item.User.Email
                    },
                    Bids = item.Bids.Select(b => new
                    {
                        b.Id,
                        b.Amount,
                        b.BidTime,
                        User = new
                        {
                            b.User.Id,
                            b.User.Email
                        }
                    }).ToList()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetItem: {ex.Message}");
                return StatusCode(500, new { error = "Failed to load auction item." });
            }
        }

        // POST: api/auctionitems
        [HttpPost]
        public async Task<ActionResult> CreateItem([FromForm] AuctionItemDto model)
        {
            if (string.IsNullOrEmpty(model.Name) || model.StartingPrice <= 0)
            {
                return BadRequest(new { error = "Name and StartingPrice are required." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            string imageUrl = null;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imageUrl = $"/images/{fileName}";
            }

            var item = new AuctionItem
            {
                Name = model.Name,
                Description = model.Description,
                StartingPrice = model.StartingPrice,
                UserId = userId,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.AuctionItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, new
            {
                item.Id,
                item.Name,
                item.Description,
                item.StartingPrice,
                item.CreatedAt,
                item.ImageUrl,
                item.BidStartTime,
                item.BidEndTime,
                item.IsClosed,
                item.WinnerEmail,
                item.WinningBidAmount,
                UserId = item.UserId,
                User = new { item.User?.Id, item.User?.Email }
            });
        }

        // PUT: api/auctionitems/start-bid/{id}
        [HttpPut("start-bid/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> StartBid(int id, [FromBody] StartBidModel model)
        {
            var item = await _context.AuctionItems.FindAsync(id);
            if (item == null) return NotFound("Auction item not found.");

            if (model.EndTime <= model.StartTime)
                return BadRequest("End time must be after start time.");

            item.BidStartTime = model.StartTime;
            item.BidEndTime = model.EndTime;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveBid",
                $"🎉 Bidding started for '{item.Name}'! Ends at {item.BidEndTime:HH:mm:ss}");

            return Ok(new
            {
                message = "Bidding started successfully.",
                item.BidStartTime,
                item.BidEndTime,
                IsBiddingOpen = item.IsBiddingOpen
            });
        }

        // DELETE: api/auctionitems/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.AuctionItems.FindAsync(id);
            if (item == null) return NotFound();

            if (!string.IsNullOrEmpty(item.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", item.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete image: {ex.Message}");
                    }
                }
            }

            _context.AuctionItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/auctionitems/end-auction/{id}
        [HttpPut("end-auction/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EndAuction(int id, [FromServices] EmailService emailService)
        {
            var item = await _context.AuctionItems
                .Include(a => a.Bids).ThenInclude(b => b.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (item == null) return NotFound("Auction item not found.");

            if (DateTime.UtcNow < (item.BidStartTime ?? DateTime.MinValue))
                return BadRequest("Auction hasn't started yet.");

            if (item.IsClosed)
                return BadRequest("Auction already ended.");

            var highestBid = item.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
            if (highestBid == null)
                return BadRequest("No bids were placed.");

            var winner = highestBid.User;
            var allBidders = item.Bids.Select(b => b.User).Distinct().ToList();

            // ✅ Send emails
            foreach (var user in allBidders)
            {
                try
                {
                    if (user.Id == winner.Id)
                    {
                        await emailService.SendEmailAsync(
                            user.Email,
                            $"🎉 Congratulations! You won '{item.Name}'",
                            $@"
<h2>Congratulations, {user.Email}!</h2>
<p>You have won the auction for <strong>{item.Name}</strong> with a bid of <strong>${highestBid.Amount:F2}</strong>.</p>
<p>Contact the admin to complete the payment.</p>
<p><em>Thank you for using BiddingBoom!</em></p>"
                        );
                    }
                    else
                    {
                        await emailService.SendEmailAsync(
                            user.Email,
                            $"🔚 Auction for '{item.Name}' has ended",
                            $@"
<h2>Hi there,</h2>
<p>The auction for <strong>{item.Name}</strong> has ended.</p>
<p>The winning bid was <strong>${highestBid.Amount:F2}</strong>.</p>
<p>Better luck next time!</p>
<p><em>Thanks for participating in BiddingBoom!</em></p>"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email to {user.Email}: {ex.Message}");
                }
            }

            // ✅ Save winner info to DB
            item.IsClosed = true;
            item.WinnerEmail = winner.Email;
            item.WinningBidAmount = highestBid.Amount;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Auction ended. Emails sent to all bidders.",
                winner = new { winner.Email, Amount = highestBid.Amount }
            });
        }

        // DTOs
        public class AuctionItemDto
        {
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal StartingPrice { get; set; }
            public IFormFile? ImageFile { get; set; }
        }

        public class StartBidModel
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }
    }
}