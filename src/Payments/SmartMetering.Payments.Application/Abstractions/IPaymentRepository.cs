using SmartMetering.Payments.Domain;

namespace SmartMetering.Payments.Application.Abstractions;

public interface IPaymentRepository
{
    Task<bool> ExistsForInvoiceAsync(Guid invoiceId, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    Task<Payment?> GetByIntentIdAsync(string intentId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
