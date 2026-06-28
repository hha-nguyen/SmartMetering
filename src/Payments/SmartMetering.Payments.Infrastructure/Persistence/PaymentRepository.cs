using Microsoft.EntityFrameworkCore;
using SmartMetering.Payments.Application.Abstractions;
using SmartMetering.Payments.Domain;

namespace SmartMetering.Payments.Infrastructure.Persistence;

public sealed class PaymentRepository(PaymentsDbContext db) : IPaymentRepository
{
    public Task<bool> ExistsForInvoiceAsync(Guid invoiceId, CancellationToken ct = default)
        => db.Payments.AnyAsync(p => p.InvoiceId == invoiceId, ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
        => await db.Payments.AddAsync(payment, ct);

    public Task<Payment?> GetByIntentIdAsync(string intentId, CancellationToken ct = default)
        => db.Payments.FirstOrDefaultAsync(p => p.StripePaymentIntentId == intentId, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
