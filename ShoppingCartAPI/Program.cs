using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using ShoppingCartAPI.Data;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(
        builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"],
        TelemetryConverter.Traces)
    .CreateLogger();

builder.Host.UseSerilog();

var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbUser = builder.Configuration["DB_USER"];
var dbPassword = builder.Configuration["DB_PASSWORD"];
var fullConnectionString = $"{baseConnectionString};User ID={dbUser};Password={dbPassword};";

builder.Services.AddDbContext<ShoppingCartDbContext>(options =>
    options.UseSqlServer(fullConnectionString));

var allowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"]?.Split(',') ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("RestrictedOrigins", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

app.UseCors("RestrictedOrigins");

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Clear();
app.Urls.Add($"http://0.0.0.0:{port}");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "Application Is Running!");

Log.Information("ShoppingCart .NET backend started successfully!");

app.Run();