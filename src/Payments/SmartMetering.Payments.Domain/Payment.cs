namespace SmartMetering.Payments.Domain;

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
}

public class Payment
{
    private Payment() { } // EF

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal Amount { get; private set; }
    public string StripePaymentIntentId { get; private set; } = default!;
    public PaymentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static Payment Create(Guid invoiceId, decimal amount, string intentId)
        => new()
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Amount = amount,
            StripePaymentIntentId = intentId,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
        };

    public void MarkSucceeded() => Status = PaymentStatus.Succeeded;
}
