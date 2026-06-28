using SmartMetering.Metering.Application.Anomalies;

namespace SmartMetering.Metering.Application.Abstractions;

public interface IAnomalyNotifier
{
    Task NotifyAsync(AnomalyDetected anomaly, CancellationToken ct = default);
}
