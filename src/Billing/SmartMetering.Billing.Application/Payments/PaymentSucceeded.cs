namespace SmartMetering.Billing.Application.Payments;

// khớp event Payments publish lên exchange "payments"
public sealed record PaymentSucceeded(
    Guid InvoiceId,
    Guid PaymentId,
    decimal Amount,
    DateTimeOffset PaidAt);
