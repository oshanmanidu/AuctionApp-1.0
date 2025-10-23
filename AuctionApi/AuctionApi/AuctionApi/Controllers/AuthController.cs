using AuctionApi.Data;
using AuctionApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuctionApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //[HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody] RegisterModel model)
        //{
        //    if (await _context.Users.AnyAsync(u => u.Username == model.Username))
        //    {
        //        return BadRequest("Username already exists");
        //    }

        //    var user = new User
        //    {
        //        Username = model.Username,
        //        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
        //        Role = model.Role.ToLower() == "admin" ? "Admin" : "User"
        //    };

        //    _context.Users.Add(user);
        //    await _context.SaveChangesAsync();

        //    return Ok("User registered successfully");
        //}

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] LoginModel model)
        //{
        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Username == model.Username);

        //    if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
        //    {
        //        return Unauthorized("Invalid credentials");
        //    }

        //    var token = GenerateJwtToken(user);
        //    return Ok(new { Token = token, Role = user.Role, Username = user.Username });
        //}

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return BadRequest("Email already exists");
            }

            var user = new User
            {
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role.ToLower() == "admin" ? "Admin" : "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email); // ← Query by Email

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token, Role = user.Role, Email = user.Email }); // ← Return Email
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                //new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Name, user.Email), // ← Store email as Name

                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    //public class RegisterModel
    //{
    //    public string Username { get; set; } = string.Empty;
    //    public string Password { get; set; } = string.Empty;
    //    public string Role { get; set; } = "User"; // default to User
    //}

    //public class LoginModel
    //{
    //    public string Username { get; set; } = string.Empty;
    //    public string Password { get; set; } = string.Empty;
    //}

    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // ← Was Username

        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // ← Was Username

        public string Password { get; set; } = string.Empty;
    }

}