namespace SmartMetering.Billing.Domain.Balances;

// Cộng dồn tiêu thụ của 1 meter, chờ xuất hoá đơn. Aggregate riêng (key = MeterId).
public class MeterBalance
{
    private MeterBalance() { } // EF

    public string MeterId { get; private set; } = default!; // PK
    public decimal AccumulatedKwh { get; private set; }

    public static MeterBalance Start(string meterId)
        => new() { MeterId = meterId, AccumulatedKwh = 0 };

    public void Add(decimal kwh) => AccumulatedKwh += kwh;

    // lấy tổng ra để xuất hoá đơn rồi reset về 0
    public decimal Drain()
    {
        var total = AccumulatedKwh;
        AccumulatedKwh = 0;
        return total;
    }
}
