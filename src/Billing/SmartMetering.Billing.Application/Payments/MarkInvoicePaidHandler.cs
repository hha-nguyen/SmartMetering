using SmartMetering.Billing.Application.Abstractions;

namespace SmartMetering.Billing.Application.Payments;

public sealed class MarkInvoicePaidHandler(IInvoiceRepository invoices)
{
    public Task HandleAsync(PaymentSucceeded evt, CancellationToken ct = default)
        => invoices.MarkPaidAsync(evt.InvoiceId, ct);
}
