using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

/// <summary>
/// DTO для создания книги. Клиент отправляет этот объект в POST-запросе.
/// Обратите внимание: здесь нет поля Id (генерируется базой данных)
/// и нет AvailableCopies (устанавливается равным TotalCopies).
/// </summary>
public class CreateBookDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(17)]
    public string? ISBN { get; set; }

    public int PublicationYear { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    public int TotalCopies { get; set; } = 1;
}

/// <summary>
/// DTO для обновления книги. Используется в PUT-запросе.
/// Структура аналогична CreateBookDto, но это сознательно отдельный класс —
/// в будущем требования к созданию и обновлению могут различаться.
/// </summary>
public class UpdateBookDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(17)]
    public string? ISBN { get; set; }

    public int PublicationYear { get; set; }

    [MaxLength(100)]
    public string? Genre { get; set; }

    public int TotalCopies { get; set; } = 1;
}