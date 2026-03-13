namespace LibraryApi.Services;

/// <summary>
/// Интерфейс сервиса кэширования.
/// Определяет 3 базовые операции: получить, сохранить, удалить.
/// 
/// Использование интерфейса вместо конкретного класса — это принцип
/// Dependency Inversion (D из SOLID): контроллеры зависят от абстракции,
/// а не от конкретной реализации Redis.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Получить объект из кэша по ключу.
    /// Возвращает null, если ключ не найден или истёк TTL.
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Сохранить объект в кэш.
    /// expiry — время жизни записи (TTL). После этого времени Redis автоматически удалит запись.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);

    /// <summary>
    /// Удалить запись из кэша по ключу.
    /// Используется при инвалидации кэша (когда данные изменились).
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Удалить все записи, ключи которых соответствуют паттерну.
    /// Например, "books:*" удалит все закэшированные книги.
    /// </summary>
    Task RemoveByPatternAsync(string pattern);
}