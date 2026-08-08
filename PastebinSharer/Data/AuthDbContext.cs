using Microsoft.EntityFrameworkCore;
using PastebinSharer.Entities;

namespace PastebinSharer.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<BlacklistedToken> BlacklistedTokens { get; set; }
    }
}