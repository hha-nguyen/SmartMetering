namespace SmartMetering.Payments.Application.Invoices;

// khớp event Billing publish lên exchange "invoices"
public sealed record InvoiceGenerated(
    Guid InvoiceId,
    string MeterId,
    DateTimeOffset PeriodEnd,
    decimal Amount);
