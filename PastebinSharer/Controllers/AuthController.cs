using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PastebinSharer.Data;
using PastebinSharer.Entities;
using PastebinSharer.Models.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;

namespace PastebinSharer.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;

        public AuthController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Email và password là bắt buộc" });
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (emailExists)
            {
                return Conflict(new { error = "Email đã tồn tại" });
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                Password = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };

            return StatusCode(201, new { message = "Đăng ký thành công", user = response });
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Email và password là bắt buộc" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Unauthorized(new { error = "Email hoặc password không đúng" });
            }

            var isMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!isMatch)
            {
                return Unauthorized(new { error = "Email hoặc password không đúng" });
            }

            var token = GenerateJwtToken(user);

            return Ok(new { message = "Login thành công", token });
        }
        
        private string GenerateJwtToken(User user)
        {
            var jwtSecret = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["Jwt:Secret"];

            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret chưa được cấu hình trong appsettings.json");
            }

            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                securityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new System.Security.Claims.Claim("userId", user.Id.ToString()),
                new System.Security.Claims.Claim("email", user.Email)
            };

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }
        
        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var userId = User.FindFirst("userId")?.Value;
            var email = User.FindFirst("email")?.Value;

            return Ok(new { message = "Bạn đã đăng nhập!", user = new { userId, email } });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var expClaim = User.FindFirst("exp")?.Value;
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim!)).UtcDateTime;

            var blacklistedToken = new BlacklistedToken
            {
                Token = token,
                ExpiresAt = expiresAt
            };

            _context.BlacklistedTokens.Add(blacklistedToken);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng xuất thành công" });
        }
    }
}