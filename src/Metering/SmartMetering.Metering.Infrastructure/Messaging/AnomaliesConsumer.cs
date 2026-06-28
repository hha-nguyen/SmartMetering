using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SmartMetering.Metering.Application.Abstractions;
using SmartMetering.Metering.Application.Anomalies;

namespace SmartMetering.Metering.Infrastructure.Messaging;

public sealed class AnomaliesConsumer(
    IServiceProvider serviceProvider,
    ILogger<AnomaliesConsumer> logger) : BackgroundService
{
    private const string ExchangeName = "anomalies";
    private const string QueueName = "metering.anomalies";

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

        logger.LogInformation("AnomaliesConsumer listening on '{Queue}'", QueueName);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var anomaly = JsonSerializer.Deserialize<AnomalyDetected>(ea.Body.Span, JsonOptions);
            if (anomaly is not null)
            {
                using var scope = serviceProvider.CreateScope();
                var notifier = scope.ServiceProvider.GetRequiredService<IAnomalyNotifier>();
                await notifier.NotifyAsync(anomaly);
            }
            await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process anomaly");
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }
}
