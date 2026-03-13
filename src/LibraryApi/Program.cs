using LibraryApi.Data;
using LibraryApi.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// ЧАСТЬ 1: РЕГИСТРАЦИЯ СЕРВИСОВ
// ============================================================

// --- PostgreSQL ---
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=librarydb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Redis ---
var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION")
    ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnection));

builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// --- Контроллеры ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// --- OpenAPI (встроенный в .NET 10, замена Swagger) ---
// Генерирует описание API в формате OpenAPI (JSON)
// Доступно по адресу /openapi/v1.json
builder.Services.AddOpenApi();

var app = builder.Build();

// ============================================================
// ЧАСТЬ 2: АВТОМАТИЧЕСКОЕ ПРИМЕНЕНИЕ МИГРАЦИЙ
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    var retries = 10;
    while (retries > 0)
    {
        try
        {
            db.Database.Migrate();
            Console.WriteLine("Database migrated successfully.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            if (retries == 0) throw;
            Console.WriteLine($"Database not ready, retrying in 3s... ({ex.Message})");
            Thread.Sleep(3000);
        }
    }
}

// ============================================================
// ЧАСТЬ 3: MIDDLEWARE PIPELINE
// ============================================================

// OpenAPI — документация API в формате JSON
// Доступна по: /openapi/v1.json
app.MapOpenApi();

// Prometheus метрики
// UseMetricServer() — эндпоинт /metrics для Prometheus
// UseHttpMetrics() — автоматический сбор метрик HTTP-запросов
app.UseMetricServer();
app.UseHttpMetrics();

// Маршрутизация к контроллерам
app.MapControllers();

app.Run();

// Для тестового проекта
public partial class Program { }