namespace SmartMetering.Payments.Application.Abstractions;

public interface IPaymentEventPublisher
{
    Task PublishSucceededAsync(Guid invoiceId, Guid paymentId, decimal amount, CancellationToken ct = default);
}
