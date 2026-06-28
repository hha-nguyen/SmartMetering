namespace SmartMetering.Payments.Application.Abstractions;

// port: tạo + confirm thanh toán qua PSP (Stripe). Trả về PaymentIntent id.
public interface IPaymentGateway
{
    Task<string> CreateAndConfirmAsync(Guid invoiceId, decimal amount, CancellationToken ct = default);
}
