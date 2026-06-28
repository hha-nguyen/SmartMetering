using Microsoft.EntityFrameworkCore;
using SmartMetering.Metering.Application.Abstractions;
using SmartMetering.Metering.Application.Readings;
using SmartMetering.Metering.Infrastructure.Persistence;
using SmartMetering.Metering.Infrastructure.Messaging;
using SmartMetering.Metering.Api.RealTime;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<MeteringDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Metering")));

builder.Services.AddScoped<IReadingRepository, ReadingRepository>();
builder.Services.AddScoped<StoreReadingHandler>();
builder.Services.AddHostedService<ReadingsConsumer>();
builder.Services.AddHealthChecks();
builder.Services.AddSignalR();
builder.Services.AddScoped<IReadingNotifier, SignalRReadingNotifier>();
builder.Services.AddScoped<IAnomalyNotifier, SignalRAnomalyNotifier>();
builder.Services.AddHostedService<AnomaliesConsumer>();

var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.UseDefaultFiles();   // phục vụ wwwroot/index.html tại "/"
app.UseStaticFiles();
app.MapHub<ReadingsHub>("/hub/readings");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MeteringDbContext>();
    db.Database.Migrate();
}

app.Run();
