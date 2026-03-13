using LibraryApi.Models;
using Xunit;

namespace LibraryApi.Tests;

/// <summary>
/// Unit-тесты для моделей данных.
/// xUnit — один из стандартных фреймворков тестирования для .NET.
/// 
/// Каждый метод с атрибутом [Fact] — это отдельный тест.
/// Assert проверяет ожидаемый результат.
/// Если Assert "падает" — тест считается неудачным.
/// </summary>
public class ModelsTests
{
    [Fact]
    public void Book_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var book = new Book();

        // Assert
        Assert.Equal(0, book.Id);
        Assert.Equal(string.Empty, book.Title);
        Assert.Equal(1, book.TotalCopies);
        Assert.Equal(1, book.AvailableCopies);
        Assert.NotNull(book.BookLoans);
        Assert.Empty(book.BookLoans);
    }

    [Fact]
    public void Book_CanSetAllProperties()
    {
        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "978-0-00-000000-0",
            PublicationYear = 2024,
            Genre = "Fiction",
            TotalCopies = 3,
            AvailableCopies = 2
        };

        Assert.Equal(1, book.Id);
        Assert.Equal("Test Book", book.Title);
        Assert.Equal("Test Author", book.Author);
        Assert.Equal("978-0-00-000000-0", book.ISBN);
        Assert.Equal(2024, book.PublicationYear);
        Assert.Equal("Fiction", book.Genre);
        Assert.Equal(3, book.TotalCopies);
        Assert.Equal(2, book.AvailableCopies);
    }

    [Fact]
    public void Reader_DefaultRegistrationDate_IsUtcNow()
    {
        var before = DateTime.UtcNow;
        var reader = new Reader();
        var after = DateTime.UtcNow;

        // Дата регистрации должна быть между "до" и "после" создания
        Assert.InRange(reader.RegistrationDate, before, after);
    }

    [Fact]
    public void Reader_CanSetProperties()
    {
        var reader = new Reader
        {
            Id = 1,
            FullName = "Иванов Иван",
            Email = "ivanov@test.com",
            Phone = "+7-900-000-0000"
        };

        Assert.Equal("Иванов Иван", reader.FullName);
        Assert.Equal("ivanov@test.com", reader.Email);
        Assert.Equal("+7-900-000-0000", reader.Phone);
    }

    [Fact]
    public void BookLoan_IsReturned_FalseByDefault()
    {
        var loan = new BookLoan
        {
            BookId = 1,
            ReaderId = 1,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        // ReturnDate == null → IsReturned == false
        Assert.False(loan.IsReturned);
        Assert.Null(loan.ReturnDate);
    }

    [Fact]
    public void BookLoan_IsReturned_TrueWhenReturnDateSet()
    {
        var loan = new BookLoan
        {
            BookId = 1,
            ReaderId = 1,
            DueDate = DateTime.UtcNow.AddDays(14),
            ReturnDate = DateTime.UtcNow  // Книга возвращена
        };

        Assert.True(loan.IsReturned);
        Assert.NotNull(loan.ReturnDate);
    }

    [Fact]
    public void BookLoan_LoanDate_DefaultIsUtcNow()
    {
        var before = DateTime.UtcNow;
        var loan = new BookLoan();
        var after = DateTime.UtcNow;

        Assert.InRange(loan.LoanDate, before, after);
    }
}