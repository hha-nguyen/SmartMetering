using System.Text.Json;
using RabbitMQ.Client;
using SmartMetering.Payments.Application.Abstractions;
using SmartMetering.Payments.Application.Payments;

namespace SmartMetering.Payments.Infrastructure.Messaging;

public sealed class RabbitMqPaymentPublisher : IPaymentEventPublisher
{
    private const string ExchangeName = "payments";

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task PublishSucceededAsync(Guid invoiceId, Guid paymentId, decimal amount, CancellationToken ct = default)
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq", UserName = "admin", Password = "admin" };
        await using var connection = await factory.CreateConnectionAsync(ct);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName, type: ExchangeType.Fanout, durable: true, cancellationToken: ct);

        var evt = new PaymentSucceeded(invoiceId, paymentId, amount, DateTimeOffset.UtcNow);
        var body = JsonSerializer.SerializeToUtf8Bytes(evt, JsonOptions);

        await channel.BasicPublishAsync(
            exchange: ExchangeName, routingKey: "", body: body, cancellationToken: ct);
    }
}
