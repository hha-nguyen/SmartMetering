namespace SmartMetering.Billing.Domain.Invoices;

public enum InvoiceStatus
{
    Unpaid,
    Paid,
}

// AGGREGATE ROOT: cửa duy nhất để thao tác Invoice + LineItems
public class Invoice
{
    private readonly List<LineItem> _lineItems = [];

    private Invoice() { } // EF

    public Guid Id { get; private set; }
    public string MeterId { get; private set; } = default!;
    public DateTimeOffset PeriodEnd { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public decimal Amount { get; private set; } // EUR — invariant: = tổng LineItems

    public IReadOnlyCollection<LineItem> LineItems => _lineItems.AsReadOnly();

    public static Invoice Create(string meterId, DateTimeOffset periodEnd, decimal kwh, decimal ratePerKwh)
    {
        if (string.IsNullOrWhiteSpace(meterId))
            throw new ArgumentException("MeterId required", nameof(meterId));
        if (kwh < 0)
            throw new ArgumentOutOfRangeException(nameof(kwh));

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            MeterId = meterId,
            PeriodEnd = periodEnd,
            Status = InvoiceStatus.Unpaid,
        };
        invoice.AddLineItem("Electricity consumption", kwh, ratePerKwh);
        return invoice;
    }

    private void AddLineItem(string description, decimal quantity, decimal unitPrice)
    {
        _lineItems.Add(LineItem.Create(description, quantity, unitPrice));
        Amount = _lineItems.Sum(li => li.Amount); // giữ invariant tổng = Σ line items
    }

    public void MarkPaid() => Status = InvoiceStatus.Paid;
}
