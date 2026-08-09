using PastebinSharer.Data;
using Microsoft.EntityFrameworkCore;

namespace PastebinSharer.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AuthDbContext dbContext)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                // Chỉ check những token bị blacklist mà chưa hết hạn thời gian sống
                var isBlacklisted = await dbContext.BlacklistedTokens
                    .AnyAsync(t => t.Token == token && t.ExpiresAt > DateTime.UtcNow);

                if (isBlacklisted)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(new { message = "Token đã bị vô hiệu hóa (đã đăng xuất)" });
                    return;
                }
            }

            await _next(context);
        }
    }
}