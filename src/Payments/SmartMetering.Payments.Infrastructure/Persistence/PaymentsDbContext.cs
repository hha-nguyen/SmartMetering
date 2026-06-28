using Microsoft.EntityFrameworkCore;
using SmartMetering.Payments.Domain;

namespace SmartMetering.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Payment>(e =>
        {
            e.ToTable("payments");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.InvoiceId).IsUnique(); // 1 invoice = 1 payment (idempotency)
            e.HasIndex(x => x.StripePaymentIntentId);
            e.Property(x => x.StripePaymentIntentId).HasMaxLength(255);
            e.Property(x => x.Amount).HasColumnType("numeric(14,2)");
            e.Property(x => x.Status).HasConversion<string>();
        });
    }
}
