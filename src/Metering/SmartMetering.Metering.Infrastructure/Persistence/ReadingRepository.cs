using SmartMetering.Metering.Application.Abstractions;
using SmartMetering.Metering.Domain.Readings;

namespace SmartMetering.Metering.Infrastructure.Persistence;

public sealed class ReadingRepository(MeteringDbContext db) : IReadingRepository
{
    public async Task AddAsync(MeterReading reading, CancellationToken ct = default)
    {
        db.Readings.Add(reading);
        await db.SaveChangesAsync(ct);
    }
}