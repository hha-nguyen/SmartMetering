namespace SmartMetering.Billing.Application.Readings;

// khớp event Ingestion publish lên exchange "readings"
public sealed record MeterReadingReceived(
    Guid ReadingId,
    string MeterId,
    DateTimeOffset Timestamp,
    decimal Kwh,
    DateTimeOffset ReceivedAt);
