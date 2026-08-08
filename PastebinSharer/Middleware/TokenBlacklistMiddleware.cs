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

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Replace("Bearer ", "");

                var isBlacklisted = await dbContext.BlacklistedTokens
                    .AnyAsync(t => t.Token == token);

                if (isBlacklisted)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { error = "Token đã bị vô hiệu hóa (đã đăng xuất)" });
                    return;
                }
            }

            await _next(context);
        }
    }
}