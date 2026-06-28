using SmartMetering.Metering.Application.Readings;

namespace SmartMetering.Metering.Application.Abstractions;

public interface IReadingNotifier
{
    Task NotifyAsync(MeterReadingReceived reading, CancellationToken ct = default);
}