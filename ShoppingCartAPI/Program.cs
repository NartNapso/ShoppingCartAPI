using Microsoft.EntityFrameworkCore;
using ShoppingCartAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ShoppingCartDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAll");

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

app.MapGet("/", () => "Hello World!");

app.Run();
