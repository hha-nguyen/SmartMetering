namespace SmartMetering.Payments.Application.Payments;

// event Payments publish lên exchange "payments" khi thanh toán thành công
public sealed record PaymentSucceeded(
    Guid InvoiceId,
    Guid PaymentId,
    decimal Amount,
    DateTimeOffset PaidAt);
