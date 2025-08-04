using Barberly.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Barberly.Infrastructure.Persistence;

public class BarberlyDbContext : DbContext
{
    public BarberlyDbContext(DbContextOptions<BarberlyDbContext> options) : base(options) { }

    public DbSet<BarberShop> BarberShops => Set<BarberShop>();
    public DbSet<Barber> Barbers => Set<Barber>();
    public DbSet<Service> Services => Set<Service>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // BarberShop
        modelBuilder.Entity<BarberShop>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.OwnsOne(e => e.Address);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(100);
            entity.Property(e => e.OpenTime);
            entity.Property(e => e.CloseTime);
            entity.Property(e => e.WorkingDays);
            entity.Property(e => e.Latitude).HasColumnType("decimal(9,6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9,6)");
            entity.HasMany(e => e.Barbers).WithOne(b => b.BarberShop).HasForeignKey(b => b.BarberShopId);
            entity.HasMany(e => e.Services).WithOne(s => s.BarberShop).HasForeignKey(s => s.BarberShopId);
        });

        // Barber
        modelBuilder.Entity<Barber>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Bio).HasMaxLength(500);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(200);
            entity.HasMany(e => e.Services).WithMany(s => s.Barbers);
        });

        // Service
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.DurationInMinutes);
            entity.HasMany(e => e.Barbers).WithMany(b => b.Services);
        });
    }
}
