using Microsoft.EntityFrameworkCore;
using SmartMetering.Billing.Application.Abstractions;
using SmartMetering.Billing.Application.Consumption;
using SmartMetering.Billing.Application.Payments;
using SmartMetering.Billing.Infrastructure.Invoices;
using SmartMetering.Billing.Infrastructure.Messaging;
using SmartMetering.Billing.Infrastructure.Outbox;
using SmartMetering.Billing.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddDbContext<BillingDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Billing")));

builder.Services.AddScoped<IMeterBalanceRepository, MeterBalanceRepository>();
builder.Services.AddScoped<AccumulateConsumptionHandler>();
builder.Services.AddScoped<IInvoiceGenerator, InvoiceGenerator>();
builder.Services.AddHostedService<ReadingsConsumer>();
builder.Services.AddHostedService<OutboxPublisher>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<MarkInvoicePaidHandler>();
builder.Services.AddHostedService<PaymentsConsumer>();

var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

// sinh hoá đơn cho 1 meter (drain balance -> Invoice + outbox, atomic)
app.MapPost("/invoices/{meterId}", async (string meterId, IInvoiceGenerator gen, CancellationToken ct) =>
{
    var id = await gen.GenerateAsync(meterId, ct);
    return id is null ? Results.NoContent() : Results.Ok(new { invoiceId = id });
});

// xem hoá đơn gần đây
app.MapGet("/invoices", async (BillingDbContext db, CancellationToken ct) =>
    await db.Invoices.Include(i => i.LineItems)
        .OrderByDescending(i => i.PeriodEnd).Take(20).ToListAsync(ct));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
    db.Database.Migrate();
}

app.Run();
