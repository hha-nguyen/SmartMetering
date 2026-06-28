using Microsoft.AspNetCore.SignalR;
using SmartMetering.Metering.Application.Abstractions;
using SmartMetering.Metering.Application.Readings;

namespace SmartMetering.Metering.Api.RealTime;

public sealed class SignalRReadingNotifier(IHubContext<ReadingsHub> hub) : IReadingNotifier
{
    public Task NotifyAsync(MeterReadingReceived reading, CancellationToken ct = default)
        => hub.Clients.All.SendAsync(
            "reading",
            new { meterId = reading.MeterId, timestamp = reading.Timestamp, kwh = reading.Kwh },
            ct);
}
