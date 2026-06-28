using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SmartMetering.Billing.Infrastructure.Persistence;

namespace SmartMetering.Billing.Infrastructure.Outbox;

// Background: poll outbox chưa gửi -> publish RabbitMQ -> đánh dấu đã gửi (at-least-once)
public sealed class OutboxPublisher(
    IServiceProvider serviceProvider,
    ILogger<OutboxPublisher> logger) : BackgroundService
{
    private const string ExchangeName = "invoices";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "rabbitmq", UserName = "admin", Password = "admin" };
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: ExchangeName, type: ExchangeType.Fanout, durable: true,
            cancellationToken: stoppingToken);

        logger.LogInformation("OutboxPublisher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();

                var pending = await db.Set<OutboxMessage>()
                    .Where(m => m.ProcessedAt == null)
                    .OrderBy(m => m.OccurredAt)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var msg in pending)
                {
                    var body = Encoding.UTF8.GetBytes(msg.Content);
                    await channel.BasicPublishAsync(
                        exchange: ExchangeName, routingKey: "", body: body,
                        cancellationToken: stoppingToken);
                    msg.ProcessedAt = DateTimeOffset.UtcNow;
                    logger.LogInformation("Published outbox {Type} {Id}", msg.Type, msg.Id);
                }

                if (pending.Count > 0)
                    await db.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
