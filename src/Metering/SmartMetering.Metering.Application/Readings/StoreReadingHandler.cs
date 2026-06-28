using SmartMetering.Metering.Application.Abstractions;
using SmartMetering.Metering.Domain.Readings;

namespace SmartMetering.Metering.Application.Readings;

public sealed class StoreReadingHandler(IReadingRepository repository, IReadingNotifier notifier)
{
    public async Task HandleAsync(MeterReadingReceived evt, CancellationToken ct = default)
    {
        var reading = MeterReading.Create(evt.ReadingId, evt.MeterId, evt.Timestamp, evt.Kwh);
        await repository.AddAsync(reading, ct);

        await notifier.NotifyAsync(evt, ct);
    }
}