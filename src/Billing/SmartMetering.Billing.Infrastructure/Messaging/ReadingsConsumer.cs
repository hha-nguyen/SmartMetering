using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartMetering.Billing.Application.Consumption;
using SmartMetering.Billing.Application.Readings;

namespace SmartMetering.Billing.Infrastructure.Messaging;

public sealed class ReadingsConsumer(
    IServiceProvider serviceProvider,
    ILogger<ReadingsConsumer> logger) : BackgroundService
{
    private const string ExchangeName = "readings";
    private const string QueueName = "billing.readings";

    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };

    private IConnection? _connection;
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq", UserName = "admin", Password = "admin" };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeName, type: ExchangeType.Fanout, durable: true,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: QueueName, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueName, exchange: ExchangeName, routingKey: "",
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnReceivedAsync;

        await _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation("Billing ReadingsConsumer listening on '{Queue}'", QueueName);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<MeterReadingReceived>(ea.Body.Span, JsonOptions);
            if (evt is not null)
            {
                using var scope = serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<AccumulateConsumptionHandler>();
                await handler.HandleAsync(evt);
            }
            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to accumulate reading");
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }
}
