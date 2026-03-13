using System.ComponentModel.DataAnnotations;

namespace LibraryApi.DTOs;

/// <summary>
/// DTO для создания выдачи. Клиент указывает только ID книги,
/// ID читателя и срок выдачи в днях. Остальное заполняется сервером.
/// </summary>
public class CreateBookLoanDto
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int ReaderId { get; set; }

    // Срок выдачи в днях. По умолчанию 14 дней.
    public int LoanDays { get; set; } = 14;
}