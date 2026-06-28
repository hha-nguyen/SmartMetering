using SmartMetering.Metering.Domain.Readings;

namespace SmartMetering.Metering.Application.Abstractions;

public interface IReadingRepository
{
    Task AddAsync(MeterReading reading, CancellationToken ct = default);
}