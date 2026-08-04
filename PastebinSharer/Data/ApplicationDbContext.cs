using Microsoft.EntityFrameworkCore;
using PastebinSharer.Entities;

namespace PastebinSharer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Paste> Pastes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Paste>()
                .HasIndex(p => p.Code)
                .IsUnique();
        }
    }
}