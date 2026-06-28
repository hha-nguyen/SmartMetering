namespace SmartMetering.Metering.Application.Anomalies;

// khớp JSON mà anomaly service (Python) publish lên exchange "anomalies"
public sealed record AnomalyDetected(
    string MeterId,
    DateTimeOffset Timestamp,
    decimal Kwh,
    string Reason);
