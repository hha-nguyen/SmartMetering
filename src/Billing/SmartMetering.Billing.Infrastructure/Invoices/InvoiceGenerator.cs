using System.Text.Json;
using SmartMetering.Billing.Application.Abstractions;
using SmartMetering.Billing.Domain.Invoices;
using SmartMetering.Billing.Infrastructure.Outbox;
using SmartMetering.Billing.Infrastructure.Persistence;

namespace SmartMetering.Billing.Infrastructure.Invoices;

public sealed class InvoiceGenerator(BillingDbContext db) : IInvoiceGenerator
{
    private const decimal RatePerKwh = 0.20m; // EUR/kWh (flat, MVP — HP/HC sau)

    public async Task<Guid?> GenerateAsync(string meterId, CancellationToken ct = default)
    {
        var balance = await db.Balances.FindAsync([meterId], ct);
        if (balance is null || balance.AccumulatedKwh <= 0)
            return null;

        var kwh = balance.Drain(); // lấy tổng tiêu thụ + reset balance về 0
        var invoice = Invoice.Create(meterId, DateTimeOffset.UtcNow, kwh, RatePerKwh);
        db.Invoices.Add(invoice);

        // OUTBOX: ghi event vào CÙNG transaction với invoice + balance reset
        var payload = JsonSerializer.Serialize(new
        {
            invoiceId = invoice.Id,
            meterId = invoice.MeterId,
            periodEnd = invoice.PeriodEnd,
            amount = invoice.Amount,
        });
        db.Outbox.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "InvoiceGenerated",
            Content = payload,
            OccurredAt = DateTimeOffset.UtcNow,
        });

        // 1 SaveChanges = 1 transaction: balance(reset) + invoice + outbox -> atomic
        await db.SaveChangesAsync(ct);
        return invoice.Id;
    }
}
