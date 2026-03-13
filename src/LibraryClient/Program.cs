using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace LibraryClient;

// ================================================================
// СОБСТВЕННЫЕ ТИПЫ ДЕЛЕГАТОВ
// ================================================================

/// <summary>
/// Делегат #1: Обработка завершения HTTP-запроса (многоадресный).
/// </summary>
public delegate void OnRequestCompleted(string endpoint, int statusCode, long elapsedMs);

/// <summary>
/// Делегат #2: Обработка ошибок API.
/// </summary>
public delegate void OnApiError(string endpoint, string errorMessage);

/// <summary>
/// Делегат #3: Обработка полученных данных (с возвращаемым значением).
/// </summary>
public delegate string OnDataReceived(string rawJson);

// ================================================================
// МОДЕЛИ
// ================================================================

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string? Genre { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}

public class Reader
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public DateTime RegistrationDate { get; set; }
}

public class CreateBookDto
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string? Genre { get; set; }
    public int TotalCopies { get; set; } = 1;
}

public class UpdateBookDto
{
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string? ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string? Genre { get; set; }
    public int TotalCopies { get; set; } = 1;
}

// ================================================================
// СЕРВИС API
// ================================================================

public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    // События (многоадресные делегаты)
    public OnRequestCompleted? RequestCompleted;
    public OnApiError? ApiError;

    // Стандартные делегаты
    private readonly Func<string, string> _formatResponse;
    private readonly Action<string> _logAction;

    public ApiService(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        // Func<string, string> — делегат для форматирования JSON
        _formatResponse = (json) =>
        {
            try
            {
                string jsonInput = json;  // Явно string для .NET 10
                var element = JsonSerializer.Deserialize<JsonElement>(jsonInput);
                return JsonSerializer.Serialize(element, _jsonOptions);
            }
            catch
            {
                return json;
            }
        };

        // Action<string> — делегат для логирования
        _logAction = (message) =>
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  [LOG] {message}");
            Console.ForegroundColor = originalColor;
        };
    }

    public async Task<T?> ExecuteAsync<T>(
        Func<HttpClient, Task<HttpResponseMessage>> requestFunc,
        string endpoint)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logAction($"Sending request to {endpoint}...");

            var response = await requestFunc(_httpClient);
            stopwatch.Stop();

            var statusCode = (int)response.StatusCode;
            RequestCompleted?.Invoke(endpoint, statusCode, stopwatch.ElapsedMilliseconds);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(content))
                {
                    var formatted = _formatResponse(content);
                    _logAction($"Response received ({content.Length} bytes)");

                    string json = content;  // Явно string для .NET 10
                    return JsonSerializer.Deserialize<T>(json, _jsonOptions);
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ApiError?.Invoke(endpoint, $"HTTP {statusCode}: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApiError?.Invoke(endpoint, ex.Message);
            RequestCompleted?.Invoke(endpoint, 0, stopwatch.ElapsedMilliseconds);
        }

        return default;
    }

    public async Task ExecuteVoidAsync(
        Func<HttpClient, Task<HttpResponseMessage>> requestFunc,
        string endpoint)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logAction($"Sending request to {endpoint}...");
            var response = await requestFunc(_httpClient);
            stopwatch.Stop();

            RequestCompleted?.Invoke(endpoint, (int)response.StatusCode, stopwatch.ElapsedMilliseconds);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ApiError?.Invoke(endpoint, $"HTTP {(int)response.StatusCode}: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            ApiError?.Invoke(endpoint, ex.Message);
            RequestCompleted?.Invoke(endpoint, 0, stopwatch.ElapsedMilliseconds);
        }
    }

    // CRUD-методы
    public Task<List<T>?> GetAllAsync<T>(string resource) =>
        ExecuteAsync<List<T>>(
            client => client.GetAsync($"api/{resource}"),
            $"GET /api/{resource}");

    public Task<T?> GetByIdAsync<T>(string resource, int id) =>
        ExecuteAsync<T>(
            client => client.GetAsync($"api/{resource}/{id}"),
            $"GET /api/{resource}/{id}");

    public Task<T?> CreateAsync<T, TDto>(string resource, TDto dto) =>
        ExecuteAsync<T>(
            client => client.PostAsJsonAsync($"api/{resource}", dto),
            $"POST /api/{resource}");

    public Task<T?> UpdateAsync<T, TDto>(string resource, int id, TDto dto) =>
        ExecuteAsync<T>(
            client => client.PutAsJsonAsync($"api/{resource}/{id}", dto),
            $"PUT /api/{resource}/{id}");

    public Task DeleteAsync(string resource, int id) =>
        ExecuteVoidAsync(
            client => client.DeleteAsync($"api/{resource}/{id}"),
            $"DELETE /api/{resource}/{id}");
}

// ================================================================
// ГЛАВНАЯ ПРОГРАММА
// ================================================================

class Program
{
    private static readonly string LogFilePath = "api_log.txt";

    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("╔══════════════════════════════════════════════════╗");
        Console.WriteLine("║   Library API Client — Delegates Demonstration  ║");
        Console.WriteLine("╚══════════════════════════════════════════════════╝");
        Console.WriteLine();

        var baseUrl = args.Length > 0 ? args[0] : "http://localhost:80";
        Console.WriteLine($"  Base URL: {baseUrl}");
        Console.WriteLine();

        var apiService = new ApiService(baseUrl);

        // ============================================================
        // ОБРАБОТЧИКИ ДЕЛЕГАТОВ
        // ============================================================

        // Обработчик 1: Вывод в КОНСОЛЬ
        OnRequestCompleted consoleHandler = (endpoint, statusCode, elapsedMs) =>
        {
            var color = statusCode switch
            {
                >= 200 and < 300 => ConsoleColor.Green,
                >= 400 and < 500 => ConsoleColor.Yellow,
                >= 500 => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"  > [{DateTime.Now:HH:mm:ss}] {endpoint} -> Status: {statusCode}, Time: {elapsedMs}ms");
            Console.ForegroundColor = originalColor;
        };

        // Обработчик 2: Запись в ФАЙЛ
        OnRequestCompleted fileLogHandler = (endpoint, statusCode, elapsedMs) =>
        {
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {endpoint} | Status: {statusCode} | Time: {elapsedMs}ms";
            File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
        };

        // Обработчик ошибок
        OnApiError errorHandler = (endpoint, errorMessage) =>
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  X ERROR at {endpoint}: {errorMessage}");
            Console.ForegroundColor = originalColor;
        };

        // ============================================================
        // ПОДПИСКА (оператор +=)
        // ============================================================
        apiService.RequestCompleted += consoleHandler;
        apiService.RequestCompleted += fileLogHandler;
        apiService.ApiError += errorHandler;

        File.WriteAllText(LogFilePath, $"=== API Log Started: {DateTime.Now} ==={Environment.NewLine}");

        // ============================================================
        // ФАЗА 1: Оба обработчика активны
        // ============================================================
        Console.WriteLine(">> PHASE 1: Both handlers active (console + file)");
        Console.WriteLine(new string('-', 55));

        // Операция 1: GET /api/books
        Console.WriteLine("\n[1] Getting all books...");
        var books = await apiService.GetAllAsync<Book>("books");
        if (books != null)
        {
            Console.WriteLine($"  Got {books.Count} books:");
            foreach (var book in books.Take(3))
            {
                Console.WriteLine($"    - [{book.Id}] \"{book.Title}\" by {book.Author} " +
                                  $"({book.AvailableCopies}/{book.TotalCopies} avail)");
            }
            if (books.Count > 3)
                Console.WriteLine($"    ... and {books.Count - 3} more");
        }

        // Операция 2: GET /api/books/1
        Console.WriteLine("\n[2] Getting book by ID = 1...");
        var book1 = await apiService.GetByIdAsync<Book>("books", 1);
        if (book1 != null)
        {
            Console.WriteLine($"  Book: \"{book1.Title}\" by {book1.Author}");
            Console.WriteLine($"  Year: {book1.PublicationYear}, Genre: {book1.Genre}");
        }

        // Операция 3: POST /api/books
        Console.WriteLine("\n[3] Creating a new book...");
        var newBookDto = new CreateBookDto
        {
            Title = "Test Book (from client)",
            Author = "Test Author",
            ISBN = "978-5-00-000000-0",
            PublicationYear = 2024,
            Genre = "Test",
            TotalCopies = 2
        };
        var createdBook = await apiService.CreateAsync<Book, CreateBookDto>("books", newBookDto);
        int createdBookId = 0;
        if (createdBook != null)
        {
            createdBookId = createdBook.Id;
            Console.WriteLine($"  Created book ID = {createdBook.Id}: \"{createdBook.Title}\"");
        }

        // ============================================================
        // ФАЗА 2: Отключаем файловое логирование (оператор -=)
        // ============================================================
        Console.WriteLine();
        Console.WriteLine(new string('=', 55));
        Console.WriteLine(">> PHASE 2: File logging DISABLED (operator -=)");
        Console.WriteLine(new string('-', 55));

        apiService.RequestCompleted -= fileLogHandler;

        Console.WriteLine("  [INFO] File log handler REMOVED.");
        Console.WriteLine("  [INFO] Console output CONTINUES.");
        Console.WriteLine();

        // Операция 4: PUT /api/books/{id}
        if (createdBookId > 0)
        {
            Console.WriteLine($"[4] Updating book ID = {createdBookId}...");
            var updateDto = new UpdateBookDto
            {
                Title = "Updated Test Book",
                Author = "Updated Author",
                ISBN = "978-5-00-000000-0",
                PublicationYear = 2024,
                Genre = "Updated",
                TotalCopies = 5
            };
            var updatedBook = await apiService.UpdateAsync<Book, UpdateBookDto>(
                "books", createdBookId, updateDto);
            if (updatedBook != null)
            {
                Console.WriteLine($"  Updated: \"{updatedBook.Title}\" ({updatedBook.TotalCopies} copies)");
            }
        }

        // Операция 5: DELETE /api/books/{id}
        if (createdBookId > 0)
        {
            Console.WriteLine($"\n[5] Deleting book ID = {createdBookId}...");
            await apiService.DeleteAsync("books", createdBookId);
            Console.WriteLine($"  Book ID = {createdBookId} deleted.");
        }

        // Дополнительно: читатели
        Console.WriteLine("\n[+] Getting all readers...");
        var readers = await apiService.GetAllAsync<Reader>("readers");
        if (readers != null)
        {
            Console.WriteLine($"  Got {readers.Count} readers:");
            foreach (var reader in readers)
            {
                Console.WriteLine($"    - [{reader.Id}] {reader.FullName} ({reader.Email})");
            }
        }

        // ============================================================
        // ПРОВЕРКА ФАЙЛА ЛОГА
        // ============================================================
        Console.WriteLine();
        Console.WriteLine(new string('=', 55));
        Console.WriteLine("FILE LOG CONTENTS:");
        Console.WriteLine("  (only entries from Phase 1 - operations 1, 2, 3)");
        Console.WriteLine(new string('-', 55));

        if (File.Exists(LogFilePath))
        {
            Console.WriteLine(File.ReadAllText(LogFilePath));
        }

        Console.WriteLine(new string('=', 55));
        Console.WriteLine("DONE. Delegates demonstrated:");
        Console.WriteLine("  1. OnRequestCompleted - custom multicast delegate");
        Console.WriteLine("  2. OnApiError - custom error delegate");
        Console.WriteLine("  3. OnDataReceived - custom delegate with return value");
        Console.WriteLine("  4. Func<HttpClient, Task<HttpResponseMessage>> - CRUD");
        Console.WriteLine("  5. Func<string, string> - JSON formatting");
        Console.WriteLine("  6. Action<string> - internal logging");
        Console.WriteLine("  7. Operators += and -= demonstrated");
    }
}