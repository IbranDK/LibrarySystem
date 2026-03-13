using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LibraryApi.Models;

/// <summary>
/// Модель "Книга". Каждый экземпляр = одна строка в таблице Books.
/// 
/// Атрибуты [Required], [MaxLength] выполняют две роли:
/// 1. Валидация входных данных (ASP.NET Core проверяет их при получении запроса)
/// 2. Ограничения в базе данных (EF Core создаёт NOT NULL, VARCHAR(N) в SQL)
/// </summary>
public class Book
{
    // Primary Key. EF Core автоматически делает поле Id первичным ключом
    // благодаря конвенции именования (Id или {ClassName}Id)
    public int Id { get; set; }

    [Required]           // NOT NULL в БД + обязательное поле при создании
    [MaxLength(200)]     // VARCHAR(200) в БД + максимум 200 символов
    public string Title { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(17)]      // ISBN-13 = 17 символов с дефисами
    public string? ISBN { get; set; }  // ? означает nullable — поле может быть пустым

    public int PublicationYear { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    public int TotalCopies { get; set; } = 1;      // Сколько всего экземпляров
    public int AvailableCopies { get; set; } = 1;   // Сколько доступно для выдачи

    // Навигационное свойство: список всех выдач этой книги.
    // [JsonIgnore] — не включаем в JSON-ответ, чтобы избежать
    // бесконечной рекурсии (Book → BookLoan → Book → BookLoan → ...)
    [JsonIgnore]
    public ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();
}