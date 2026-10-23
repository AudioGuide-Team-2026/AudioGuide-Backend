using Microsoft.EntityFrameworkCore;
using Guide.Api.Models;

namespace Guide.Api.Data;

public sealed class GuideDbContext(DbContextOptions<GuideDbContext> options) : DbContext(options)
{
    public DbSet<Place> Places => Set<Place>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Place>(entity =>
        {
            entity.ToTable("places");
            entity.HasKey(place => place.Id);
            entity.Property(place => place.Name).HasMaxLength(200).IsRequired();
            entity.Property(place => place.Description).HasMaxLength(5000);
            entity.Property(place => place.Latitude).HasPrecision(9, 6);
            entity.Property(place => place.Longitude).HasPrecision(9, 6);
        });
    }
}
