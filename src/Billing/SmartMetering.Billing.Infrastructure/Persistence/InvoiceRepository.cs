using Microsoft.EntityFrameworkCore;
using SmartMetering.Billing.Application.Abstractions;

namespace SmartMetering.Billing.Infrastructure.Persistence;

public sealed class InvoiceRepository(BillingDbContext db) : IInvoiceRepository
{
    public async Task MarkPaidAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await db.Invoices.FindAsync([invoiceId], ct);
        if (invoice is null)
            return;

        invoice.MarkPaid();
        await db.SaveChangesAsync(ct);
    }
}
