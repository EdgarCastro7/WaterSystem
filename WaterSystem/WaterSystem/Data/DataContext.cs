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

        public DbSet<Invoice> Invoices { get; set; }


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

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Meter)
                .WithMany()
                .HasForeignKey(i => i.MeterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Consumption)
                .WithMany()
                .HasForeignKey(i => i.ConsumptionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consumption>()
                .Property(c => c.Volume)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Price)
                .HasPrecision(10, 2);
        }
    }
}

