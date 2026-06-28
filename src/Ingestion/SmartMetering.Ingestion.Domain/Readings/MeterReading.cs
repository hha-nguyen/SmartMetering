namespace SmartMetering.Ingestion.Domain.Readings;

public class MeterReading
{
    // ctor rỗng cho EF Core materialize (private -> code ngoài buộc dùng Create)
    private MeterReading() { }

    public Guid Id { get; private set; }
    public string MeterId { get; private set; } = default!;
    public DateTimeOffset Timestamp { get; private set; }
    public decimal Kwh { get; private set; }

    // factory: nơi DUY NHẤT tạo MeterReading mới -> luôn hợp lệ
    public static MeterReading Create(string meterId, DateTimeOffset timestamp, decimal kwh)
    {
        if (string.IsNullOrWhiteSpace(meterId))
            throw new ArgumentException("MeterId is required", nameof(meterId));

        if (kwh < 0)
            throw new ArgumentOutOfRangeException(nameof(kwh), "kWh cannot be negative");

        return new MeterReading
        {
            Id = Guid.NewGuid(),
            MeterId = meterId,
            Timestamp = timestamp,
            Kwh = kwh,
        };
    }
}
