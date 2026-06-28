namespace SmartMetering.Billing.Application.Abstractions;

public interface IInvoiceRepository
{
    Task MarkPaidAsync(Guid invoiceId, CancellationToken ct = default);
}
