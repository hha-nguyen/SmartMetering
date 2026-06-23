using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
builder.Service
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
