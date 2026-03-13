using System.ComponentModel.DataAnnotations;

namespace LibraryApi.Models;

/// <summary>
/// Модель "Выдача книги". Связующая таблица между Book и Reader.
/// Каждая запись = факт выдачи конкретной книги конкретному читателю.
/// 
/// Связь: Book (1) ← (N) BookLoan (N) → (1) Reader
/// Одна книга может быть выдана многим читателям (в разное время).
/// Один читатель может взять много книг.
/// </summary>
public class BookLoan
{
    public int Id { get; set; }

    // Foreign Key на таблицу Books
    [Required]
    public int BookId { get; set; }
    // Навигационное свойство — EF Core автоматически выполнит JOIN при запросе
    public Book Book { get; set; } = null!;  // null! говорит компилятору: "я знаю, что это не null после загрузки из БД"

    // Foreign Key на таблицу Readers
    [Required]
    public int ReaderId { get; set; }
    public Reader Reader { get; set; } = null!;

    public DateTime LoanDate { get; set; } = DateTime.UtcNow;   // Когда выдана
    public DateTime DueDate { get; set; }                        // Когда нужно вернуть
    public DateTime? ReturnDate { get; set; }                    // Когда фактически вернули (null = не вернули)

    // Вычисляемое свойство — не хранится в БД, вычисляется на лету
    public bool IsReturned => ReturnDate.HasValue;
}