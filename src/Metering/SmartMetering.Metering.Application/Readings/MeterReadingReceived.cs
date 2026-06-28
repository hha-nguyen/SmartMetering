namespace SmartMetering.Metering.Application.Readings;

// khớp JSON mà Ingestion publish lên queue "readings"
public sealed record MeterReadingReceived(
    Guid ReadingId,
    string MeterId,
    DateTimeOffset Timestamp,
    decimal Kwh,
    DateTimeOffset ReceivedAt
);