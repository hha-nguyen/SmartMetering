using Microsoft.EntityFrameworkCore;
using SmartMetering.Metering.Domain.Readings;

namespace SmartMetering.Metering.Infrastructure.Persistence;

public sealed class MeteringDbContext(DbContextOptions<MeteringDbContext> options) : DbContext(options)
{
    public DbSet<MeterReading> Readings => Set<MeterReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeterReading>(b =>
        {
            b.ToTable("readings");
            b.HasKey(r => new { r.MeterId, r.Timestamp });
            b.Property(r => r.Id).HasColumnName("id");
            b.Property(r => r.MeterId).HasColumnName("meter_id").IsRequired().HasMaxLength(64);
            b.Property(r => r.Timestamp).HasColumnName("timestamp").IsRequired();
            b.Property(r => r.Kwh).HasColumnName("kwh").HasColumnType("numeric(12,3)");
        });
    }
}