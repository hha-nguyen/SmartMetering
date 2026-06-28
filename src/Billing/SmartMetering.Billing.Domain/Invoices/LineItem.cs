namespace SmartMetering.Billing.Domain.Invoices;

// Entity NẰM TRONG aggregate Invoice (không truy cập trực tiếp từ ngoài)
public class LineItem
{
    private LineItem() { } // EF

    public Guid Id { get; private set; }
    public string Description { get; private set; } = default!;
    public decimal Quantity { get; private set; } // kWh
    public decimal UnitPrice { get; private set; } // EUR/kWh
    public decimal Amount { get; private set; } // = Quantity * UnitPrice

    public static LineItem Create(string description, decimal quantity, decimal unitPrice)
        => new()
        {
            Id = Guid.NewGuid(),
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Amount = quantity * unitPrice,
        };
}
