using SmartMetering.Ingestion.Domain.Readings;

namespace SmartMetering.Ingestion.Application.Abstractions;

public interface IReadingStore
{
    Task SaveAsync(MeterReading reading, CancellationToken cancellationToken = default);
}