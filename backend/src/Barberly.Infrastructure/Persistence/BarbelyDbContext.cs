using Microsoft.EntityFrameworkCore;
using Barberly.Domain.Common;

namespace Barberly.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for Barberly application
/// Follows Clean Architecture principles with proper entity configurations
/// </summary>
public class BarbelyDbContext : DbContext
{
    public BarbelyDbContext(DbContextOptions<BarbelyDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure all entities here
        ConfigureCommonProperties(modelBuilder);
        
        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureCommonProperties(ModelBuilder modelBuilder)
    {
        // Configure common properties for all entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(Entity).IsAssignableFrom(entityType.ClrType))
            {
                // Configure audit fields
                modelBuilder.Entity(entityType.ClrType)
                    .Property("CreatedAt")
                    .IsRequired();
                
                modelBuilder.Entity(entityType.ClrType)
                    .Property("UpdatedAt")
                    .IsRequired();
            }
        }
    }
}
