using SmartMetering.Payments.Application.Abstractions;
using SmartMetering.Payments.Application.Invoices;
using SmartMetering.Payments.Domain;

namespace SmartMetering.Payments.Application.Payments;

public sealed class CreatePaymentHandler(IPaymentGateway gateway, IPaymentRepository repo)
{
    public async Task HandleAsync(InvoiceGenerated evt, CancellationToken ct = default)
    {
        // idempotent: đã có payment cho invoice này thì bỏ qua (event có thể tới >1 lần)
        if (await repo.ExistsForInvoiceAsync(evt.InvoiceId, ct))
            return;

        var intentId = await gateway.CreateAndConfirmAsync(evt.InvoiceId, evt.Amount, ct);
        await repo.AddAsync(Payment.Create(evt.InvoiceId, evt.Amount, intentId), ct);
        await repo.SaveChangesAsync(ct);
    }
}
