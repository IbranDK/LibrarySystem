using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LibraryApi.Models;

/// <summary>
/// Модель "Читатель". Представляет зарегистрированного пользователя библиотеки.
/// </summary>
public class Reader
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]       // Валидация формата email (должен содержать @)
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Phone { get; set; }

    // Дата регистрации. По умолчанию — текущее время в UTC
    // (UTC используется для единообразия, чтобы не зависеть от часового пояса сервера)
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();
}