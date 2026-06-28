namespace SmartMetering.Ingestion.Application.Readings;

// Integration event: "đã nhận 1 số đo" — hợp đồng Ingestion publish lên broker
public sealed record MeterReadingReceived(
    Guid ReadingId,
    string MeterId,
    DateTimeOffset Timestamp,
    decimal Kwh,
    DateTimeOffset ReceivedAt
);