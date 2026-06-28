namespace SmartMetering.Metering.Domain.Readings;

public class MeterReading
{
    private MeterReading() { } // cho EF

    public Guid Id { get; private set; }
    public string MeterId { get; private set; } = default!;
    public DateTimeOffset Timestamp { get; private set; }
    public decimal Kwh { get; private set; }

    public static MeterReading Create(Guid id, string meterId, DateTimeOffset timestamp, decimal kwh)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id required", nameof(id));
        if (string.IsNullOrWhiteSpace(meterId)) throw new ArgumentException("MeterId required", nameof(meterId));
        if (kwh < 0) throw new ArgumentOutOfRangeException(nameof(kwh));

        return new MeterReading { Id = id, MeterId = meterId, Timestamp = timestamp, Kwh = kwh };
    }
}