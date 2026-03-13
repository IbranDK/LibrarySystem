using LibraryApi.Data;
using LibraryApi.DTOs;
using LibraryApi.Models;
using LibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers;

/// <summary>
/// Контроллер для управления выдачами книг.
/// Содержит бизнес-логику: проверка доступности экземпляров,
/// уменьшение/увеличение счётчика при выдаче/возврате.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BookLoansController : ControllerBase
{
    private readonly LibraryDbContext _context;
    private readonly ICacheService _cache;
    private readonly ILogger<BookLoansController> _logger;

    public BookLoansController(
        LibraryDbContext context,
        ICacheService cache,
        ILogger<BookLoansController> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookLoan>>> GetAll()
    {
        // Include — выполняет JOIN, чтобы загрузить связанные Book и Reader
        var loans = await _context.BookLoans
            .Include(bl => bl.Book)
            .Include(bl => bl.Reader)
            .AsNoTracking()
            .ToListAsync();

        return Ok(loans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookLoan>> GetById(int id)
    {
        var loan = await _context.BookLoans
            .Include(bl => bl.Book)
            .Include(bl => bl.Reader)
            .FirstOrDefaultAsync(bl => bl.Id == id);

        if (loan == null)
            return NotFound(new { message = $"BookLoan with id {id} not found" });

        return Ok(loan);
    }

    /// <summary>
    /// Выдать книгу читателю.
    /// Бизнес-логика:
    /// 1. Проверить, что книга существует
    /// 2. Проверить, что есть доступные экземпляры
    /// 3. Проверить, что читатель существует
    /// 4. Создать запись о выдаче
    /// 5. Уменьшить счётчик доступных экземпляров
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BookLoan>> Create([FromBody] CreateBookLoanDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var book = await _context.Books.FindAsync(dto.BookId);
        if (book == null)
            return NotFound(new { message = $"Book with id {dto.BookId} not found" });

        if (book.AvailableCopies <= 0)
            return BadRequest(new { message = "No available copies of this book" });

        var reader = await _context.Readers.FindAsync(dto.ReaderId);
        if (reader == null)
            return NotFound(new { message = $"Reader with id {dto.ReaderId} not found" });

        var loan = new BookLoan
        {
            BookId = dto.BookId,
            ReaderId = dto.ReaderId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(dto.LoanDays)
        };

        book.AvailableCopies--;  // Уменьшаем доступные экземпляры

        _context.BookLoans.Add(loan);
        await _context.SaveChangesAsync();

        // Инвалидируем кэш книг (доступные экземпляры изменились)
        await _cache.RemoveAsync("books:all");
        await _cache.RemoveAsync($"books:{dto.BookId}");

        _logger.LogInformation(
            "Book {BookId} loaned to reader {ReaderId}, due {DueDate}",
            dto.BookId, dto.ReaderId, loan.DueDate);

        return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
    }

    /// <summary>
    /// Вернуть книгу. Специальный эндпоинт, не стандартный CRUD.
    /// PUT /api/bookloans/{id}/return
    /// </summary>
    [HttpPut("{id}/return")]
    public async Task<ActionResult<BookLoan>> ReturnBook(int id)
    {
        var loan = await _context.BookLoans
            .Include(bl => bl.Book)
            .FirstOrDefaultAsync(bl => bl.Id == id);

        if (loan == null)
            return NotFound(new { message = $"BookLoan with id {id} not found" });

        if (loan.ReturnDate.HasValue)
            return BadRequest(new { message = "This book has already been returned" });

        loan.ReturnDate = DateTime.UtcNow;
        loan.Book.AvailableCopies++;  // Возвращаем экземпляр в доступные

        await _context.SaveChangesAsync();

        await _cache.RemoveAsync("books:all");
        await _cache.RemoveAsync($"books:{loan.BookId}");

        _logger.LogInformation("Book {BookId} returned", loan.BookId);

        return Ok(loan);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var loan = await _context.BookLoans.FindAsync(id);
        if (loan == null)
            return NotFound(new { message = $"BookLoan with id {id} not found" });

        _context.BookLoans.Remove(loan);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}