using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Data;

/// <summary>
/// Контекст базы данных. EF Core использует этот класс для:
/// 1. Создания/изменения структуры БД (миграции)
/// 2. Выполнения запросов (SELECT, INSERT, UPDATE, DELETE)
/// 3. Отслеживания изменений объектов (change tracking)
/// 
/// Каждое свойство DbSet<T> соответствует одной таблице в БД.
/// Имя свойства = имя таблицы (Books, Readers, BookLoans).
/// </summary>
public class LibraryDbContext : DbContext
{
    // Конструктор принимает настройки (строку подключения и т.д.)
    // Они передаются через Dependency Injection в Program.cs
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
        : base(options) { }

    // Каждый DbSet = одна таблица в БД
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Reader> Readers => Set<Reader>();
    public DbSet<BookLoan> BookLoans => Set<BookLoan>();

    /// <summary>
    /// Метод вызывается при создании модели БД.
    /// Здесь мы настраиваем:
    /// - ключи и индексы
    /// - связи между таблицами
    /// - начальные данные (seed data)
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== Настройка таблицы Books =====
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);                          // PRIMARY KEY
            entity.HasIndex(b => b.ISBN).IsUnique();           // UNIQUE INDEX на ISBN
            entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Author).IsRequired().HasMaxLength(100);
        });

        // ===== Настройка таблицы Readers =====
        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Email).IsUnique();          // Email должен быть уникальным
            entity.Property(r => r.FullName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Email).IsRequired().HasMaxLength(150);
        });

        // ===== Настройка таблицы BookLoans (связи) =====
        modelBuilder.Entity<BookLoan>(entity =>
        {
            entity.HasKey(bl => bl.Id);

            // Связь: BookLoan → Book (многие к одному)
            // OnDelete(Restrict) — нельзя удалить книгу, если есть незакрытые выдачи
            entity.HasOne(bl => bl.Book)
                  .WithMany(b => b.BookLoans)
                  .HasForeignKey(bl => bl.BookId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Связь: BookLoan → Reader (многие к одному)
            entity.HasOne(bl => bl.Reader)
                  .WithMany(r => r.BookLoans)
                  .HasForeignKey(bl => bl.ReaderId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Seed Data — начальные тестовые данные =====
        // Эти данные будут вставлены в БД при первом применении миграции.
        // ВАЖНО: Id указываются явно, чтобы EF Core мог отслеживать их в миграциях.
        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = 1,
                Title = "Война и мир",
                Author = "Лев Толстой",
                ISBN = "978-5-17-090000-1",
                PublicationYear = 1869,
                Genre = "Роман",
                TotalCopies = 5,
                AvailableCopies = 5
            },
            new Book
            {
                Id = 2,
                Title = "Преступление и наказание",
                Author = "Фёдор Достоевский",
                ISBN = "978-5-17-090000-2",
                PublicationYear = 1866,
                Genre = "Роман",
                TotalCopies = 3,
                AvailableCopies = 3
            },
            new Book
            {
                Id = 3,
                Title = "Мастер и Маргарита",
                Author = "Михаил Булгаков",
                ISBN = "978-5-17-090000-3",
                PublicationYear = 1967,
                Genre = "Роман",
                TotalCopies = 4,
                AvailableCopies = 4
            },
            new Book
            {
                Id = 4,
                Title = "Евгений Онегин",
                Author = "Александр Пушкин",
                ISBN = "978-5-17-090000-4",
                PublicationYear = 1833,
                Genre = "Поэма",
                TotalCopies = 2,
                AvailableCopies = 2
            },
            new Book
            {
                Id = 5,
                Title = "Анна Каренина",
                Author = "Лев Толстой",
                ISBN = "978-5-17-090000-5",
                PublicationYear = 1877,
                Genre = "Роман",
                TotalCopies = 3,
                AvailableCopies = 3
            }
        );

        modelBuilder.Entity<Reader>().HasData(
            new Reader
            {
                Id = 1,
                FullName = "Иванов Иван Иванович",
                Email = "ivanov@example.com",
                Phone = "+7-900-111-1111",
                RegistrationDate = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new Reader
            {
                Id = 2,
                FullName = "Петрова Мария Сергеевна",
                Email = "petrova@example.com",
                Phone = "+7-900-222-2222",
                RegistrationDate = new DateTime(2024, 2, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new Reader
            {
                Id = 3,
                FullName = "Сидоров Алексей Петрович",
                Email = "sidorov@example.com",
                Phone = "+7-900-333-3333",
                RegistrationDate = new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}