using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using SmartMetering.Ingestion.Application.Abstractions;
using SmartMetering.Ingestion.Application.Readings;
using SmartMetering.Ingestion.Infrastructure.Readings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IReadingStore, RabbitMqReadingStore>();
builder.Services.AddScoped<IngestReadingHandler>();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

// Endpoint nghiệp vụ: nhận 1 số đo
app.MapPost("/readings", async (
    IngestReadingRequest request,
    IngestReadingHandler handler,
    CancellationToken ct) =>
{
    await handler.HandleAsync(request, ct);
    return Results.Accepted();
});

app.MapGet("/", () => "Ingestion service is running");

// LIVENESS: "còn sống không?" — KHÔNG check dependency
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });

// READINESS: "sẵn sàng nhận traffic chưa?" — chỉ chạy check gắn tag "ready"
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") }
);

app.Run();
