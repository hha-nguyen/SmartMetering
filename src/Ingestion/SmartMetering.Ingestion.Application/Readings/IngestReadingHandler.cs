using SmartMetering.Ingestion.Application.Abstractions;
using SmartMetering.Ingestion.Domain.Readings;

namespace SmartMetering.Ingestion.Application.Readings;

// DTO đầu vào của use case
public sealed record IngestReadingRequest(string MeterId, DateTimeOffset Timestamp, decimal Kwh);

// Use case: nhận request -> tạo domain object (validate) -> nhờ port lưu
public sealed class IngestReadingHandler(IReadingStore store)
{
    public async Task HandleAsync(
        IngestReadingRequest request,
        CancellationToken cancellationToken = default)
    {
        var reading = MeterReading.Create(request.MeterId, request.Timestamp, request.Kwh);
        await store.SaveAsync(reading, cancellationToken);
    }
}