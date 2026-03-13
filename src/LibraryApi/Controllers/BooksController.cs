using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

/// <summary>
/// Контроллер для управления книгами.
/// 
/// [ApiController] — включает автоматическую валидацию моделей,
/// привязку параметров из тела запроса и генерацию ответов 400 Bad Request.
/// 
/// [Route("api/[controller]")] — определяет базовый URL.
/// [controller] заменяется на имя контроллера без суффикса Controller.
/// Результат: /api/books
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly LibraryDbContext _context;     // Доступ к базе данных
    private readonly ICacheService _cache;          // Доступ к кэшу Redis
    private readonly ILogger<BooksController> _logger;  // Логирование

    // Ключи кэша. Используем константы для единообразия.
    private const string AllBooksCacheKey = "books:all";
    private const string BookCacheKeyPrefix = "books:";

    // Dependency Injection: ASP.NET Core автоматически создаёт и передаёт
    // экземпляры context, cache и logger при создании контроллера.
    public BooksController(
        LibraryDbContext context,
        ICacheService cache,
        ILogger<BooksController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/books — получить список всех книг.
    /// 
    /// Логика кэширования:
    /// 1. Проверяем, есть ли данные в Redis по ключу "books:all"
    /// 2. Если есть (cache hit) — возвращаем их, минуя базу данных
    /// 3. Если нет (cache miss) — идём в PostgreSQL, получаем данные,
    ///    сохраняем в Redis с TTL 5 минут и возвращаем клиенту
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetAll()
    {
        // Шаг 1: Проверяем кэш
        var cached = await _cache.GetAsync<List<Book>>(AllBooksCacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Books retrieved from CACHE (cache hit)");
            return Ok(cached);  // 200 OK + данные из кэша
        }

        // Шаг 2: Кэш пуст — идём в базу данных
        // AsNoTracking() — отключает отслеживание изменений для чтения
        // (ускоряет запрос, т.к. EF Core не создаёт прокси-объекты)
        var books = await _context.Books.AsNoTracking().ToListAsync();

        // Шаг 3: Сохраняем результат в кэш
        await _cache.SetAsync(AllBooksCacheKey, books, TimeSpan.FromMinutes(5));
        _logger.LogInformation("Books retrieved from DATABASE and cached");

        return Ok(books);  // 200 OK
    }

    /// <summary>
    /// GET /api/books/{id} — получить книгу по идентификатору.
    /// Также использует кэширование (каждая книга кэшируется отдельно).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetById(int id)
    {
        var cacheKey = $"{BookCacheKeyPrefix}{id}";  // например, "books:3"

        var cached = await _cache.GetAsync<Book>(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Book {Id} retrieved from CACHE", id);
            return Ok(cached);
        }

        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound(new { message = $"Book with id {id} not found" });  // 404

        await _cache.SetAsync(cacheKey, book, TimeSpan.FromMinutes(5));
        _logger.LogInformation("Book {Id} retrieved from DATABASE and cached", id);

        return Ok(book);  // 200 OK
    }

    /// <summary>
    /// POST /api/books — создать новую книгу.
    /// 
    /// [FromBody] — ASP.NET Core десериализует JSON из тела запроса в объект CreateBookDto.
    /// Если JSON невалиден (отсутствуют обязательные поля), метод вернёт 400 Bad Request
    /// автоматически благодаря [ApiController].
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Book>> Create([FromBody] CreateBookDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);  // 400 с описанием ошибок валидации

        // Создаём объект модели из DTO
        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            PublicationYear = dto.PublicationYear,
            Genre = dto.Genre,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies  // При создании все копии доступны
        };

        _context.Books.Add(book);          // Помечаем для INSERT
        await _context.SaveChangesAsync();  // Выполняем INSERT в БД

        // ИНВАЛИДАЦИЯ КЭША: список книг изменился, старый кэш неактуален
        await _cache.RemoveAsync(AllBooksCacheKey);

        _logger.LogInformation("Book created with id {Id}", book.Id);

        // 201 Created + заголовок Location с URL новой книги
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }

    /// <summary>
    /// PUT /api/books/{id} — обновить существующую книгу.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Book>> Update(int id, [FromBody] UpdateBookDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound(new { message = $"Book with id {id} not found" });

        // Обновляем поля
        book.Title = dto.Title;
        book.Author = dto.Author;
        book.ISBN = dto.ISBN;
        book.PublicationYear = dto.PublicationYear;
        book.Genre = dto.Genre;

        // Пересчитываем доступные копии при изменении общего количества
        var difference = dto.TotalCopies - book.TotalCopies;
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies += difference;

        await _context.SaveChangesAsync();  // Выполняем UPDATE в БД

        // Инвалидируем оба ключа кэша: список и конкретную книгу
        await _cache.RemoveAsync(AllBooksCacheKey);
        await _cache.RemoveAsync($"{BookCacheKeyPrefix}{id}");

        _logger.LogInformation("Book {Id} updated", id);
        return Ok(book);  // 200 OK
    }

    /// <summary>
    /// DELETE /api/books/{id} — удалить книгу.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
            return NotFound(new { message = $"Book with id {id} not found" });

        _context.Books.Remove(book);        // Помечаем для DELETE
        await _context.SaveChangesAsync();   // Выполняем DELETE в БД

        // Инвалидируем кэш
        await _cache.RemoveAsync(AllBooksCacheKey);
        await _cache.RemoveAsync($"{BookCacheKeyPrefix}{id}");

        _logger.LogInformation("Book {Id} deleted", id);
        return NoContent();  // 204 No Content — стандартный ответ при успешном удалении
    }
}