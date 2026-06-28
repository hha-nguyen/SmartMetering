using Microsoft.AspNetCore.SignalR;
using SmartMetering.Metering.Application.Abstractions;
using SmartMetering.Metering.Application.Anomalies;

namespace SmartMetering.Metering.Api.RealTime;

public sealed class SignalRAnomalyNotifier(IHubContext<ReadingsHub> hub) : IAnomalyNotifier
{
    public Task NotifyAsync(AnomalyDetected anomaly, CancellationToken ct = default)
        => hub.Clients.All.SendAsync(
            "anomaly",
            new
            {
                meterId = anomaly.MeterId,
                timestamp = anomaly.Timestamp,
                kwh = anomaly.Kwh,
                reason = anomaly.Reason,
            },
            ct);
}
