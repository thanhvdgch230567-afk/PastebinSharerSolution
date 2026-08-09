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

        // 🟢 Bổ sung DbSet cho Paste để PasteService truy vấn dữ liệu
        public DbSet<Paste> Pastes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ràng buộc Email là duy nhất trong CSDL
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Cấu hình bảng Pastes
            modelBuilder.Entity<Paste>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Code).IsUnique(); // Mã Paste là duy nhất
            });
        }
    }
}