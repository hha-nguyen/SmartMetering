using Microsoft.EntityFrameworkCore;
using SmartMetering.Payments.Application.Abstractions;
using SmartMetering.Payments.Application.Payments;
using SmartMetering.Payments.Infrastructure.Messaging;
using SmartMetering.Payments.Infrastructure.Persistence;
using SmartMetering.Payments.Infrastructure.Stripe;
using SmartMetering.Payments.Domain;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddDbContext<PaymentsDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Payments")));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentGateway, StripePaymentGateway>();
builder.Services.AddScoped<IPaymentEventPublisher, RabbitMqPaymentPublisher>();
builder.Services.AddScoped<CreatePaymentHandler>();
builder.Services.AddHostedService<InvoicesConsumer>();

var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// Stripe gọi webhook khi thanh toán xong
app.MapPost("/webhooks/stripe", async (
    HttpRequest req, IPaymentRepository repo, IPaymentEventPublisher publisher,
    IConfiguration config, CancellationToken ct) =>
{
    var json = await new StreamReader(req.Body).ReadToEndAsync(ct);
    Event stripeEvent;
    try
    {
        // verify chữ ký Stripe (chống giả mạo) bằng webhook secret
        stripeEvent = EventUtility.ConstructEvent(
            json, req.Headers["Stripe-Signature"], config["Stripe:WebhookSecret"]);
    }
    catch
    {
        return Results.BadRequest();
    }

    if (stripeEvent.Type == "payment_intent.succeeded"
        && stripeEvent.Data.Object is PaymentIntent intent)
    {
        var payment = await repo.GetByIntentIdAsync(intent.Id, ct);
        if (payment is not null && payment.Status != PaymentStatus.Succeeded) // idempotent
        {
            payment.MarkSucceeded();
            await repo.SaveChangesAsync(ct);
            await publisher.PublishSucceededAsync(payment.InvoiceId, payment.Id, payment.Amount, ct);
        }
    }
    return Results.Ok();
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();
    db.Database.Migrate();
}

app.Run();
