using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SmartMetering.Ingestion.Application.Abstractions;
using SmartMetering.Ingestion.Application.Readings;
using SmartMetering.Ingestion.Domain.Readings;

namespace SmartMetering.Ingestion.Infrastructure.Readings;

public sealed class RabbitMqReadingStore(ILogger<RabbitMqReadingStore> logger) : IReadingStore
{
    private const string ExchangeName = "readings";

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task SaveAsync(MeterReading reading, CancellationToken cancellationToken = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq", // tên Service trong k3d
            UserName = "admin",
            Password = "admin",
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName, type: ExchangeType.Fanout, durable: true,
            cancellationToken: cancellationToken);

        var evt = new MeterReadingReceived(
            reading.Id, reading.MeterId, reading.Timestamp, reading.Kwh, DateTimeOffset.UtcNow);

        var body = JsonSerializer.SerializeToUtf8Bytes(evt, JsonOptions);

        await channel.BasicPublishAsync(
            exchange: ExchangeName, routingKey: "", body: body,
            cancellationToken: cancellationToken);

        logger.LogInformation("Published MeterReadingReceived for Meter={MeterId}", reading.MeterId);
    }
}