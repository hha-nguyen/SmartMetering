using Microsoft.EntityFrameworkCore;
using SmartMetering.Billing.Domain.Balances;
using SmartMetering.Billing.Domain.Invoices;
using SmartMetering.Billing.Infrastructure.Outbox;

namespace SmartMetering.Billing.Infrastructure.Persistence;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<MeterBalance> Balances => Set<MeterBalance>();
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<MeterBalance>(e =>
        {
            e.ToTable("meter_balances");
            e.HasKey(x => x.MeterId);
            e.Property(x => x.MeterId).HasMaxLength(64);
            e.Property(x => x.AccumulatedKwh).HasColumnType("numeric(14,3)");
        });

        b.Entity<Invoice>(e =>
        {
            e.ToTable("invoices");
            e.HasKey(x => x.Id);
            e.Property(x => x.MeterId).IsRequired().HasMaxLength(64);
            e.Property(x => x.Amount).HasColumnType("numeric(14,2)");
            e.Property(x => x.Status).HasConversion<string>(); // enum -> text

            // aggregate: LineItems lộ qua IReadOnlyCollection, EF dùng backing field _lineItems
            e.HasMany(x => x.LineItems).WithOne().HasForeignKey("InvoiceId").OnDelete(DeleteBehavior.Cascade);
            e.Navigation(x => x.LineItems).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        b.Entity<LineItem>(e =>
        {
            e.ToTable("line_items");
            e.HasKey(x => x.Id);
            e.Property(x => x.Description).HasMaxLength(200);
            e.Property(x => x.Quantity).HasColumnType("numeric(14,3)");
            e.Property(x => x.UnitPrice).HasColumnType("numeric(14,4)");
            e.Property(x => x.Amount).HasColumnType("numeric(14,2)");
        });

        b.Entity<OutboxMessage>(e =>
        {
            e.ToTable("outbox_messages");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ProcessedAt);
        });
    }
}
