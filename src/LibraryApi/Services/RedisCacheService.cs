using System.Text.Json;
using StackExchange.Redis;

namespace LibraryApi.Services;

/// <summary>
/// Реализация кэширования через Redis.
/// 
/// Redis хранит данные в формате "ключ → строка".
/// Мы сериализуем C#-объекты в JSON перед сохранением
/// и десериализуем обратно при чтении.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(5);

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return default;

        // ИСПРАВЛЕНИЕ: явно приводим RedisValue к string,
        // чтобы компилятор .NET 10 не путал перегрузки
        // Deserialize(string, ...) и Deserialize(ReadOnlySpan<byte>, ...)
        string jsonString = value.ToString();
        return JsonSerializer.Deserialize<T>(jsonString);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var serialized = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, serialized, expiry ?? _defaultExpiry);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern).ToArray();

        if (keys.Length > 0)
        {
            await _db.KeyDeleteAsync(keys);
        }
    }
}