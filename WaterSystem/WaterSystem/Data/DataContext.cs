using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WaterSystem.Data.Entities;

namespace WaterSystem.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> dbContext) : base(dbContext)
        {
            
        }

        public DbSet<Consumption> Consumptions { get; set; }

        public DbSet<Meter> Meters { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Meter>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consumption>()
                .HasOne(c => c.Meter)
                .WithMany(m => m.Consumptions)
                .HasForeignKey(c => c.MeterId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

