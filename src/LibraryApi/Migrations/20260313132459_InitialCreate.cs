using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibraryApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ISBN = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    PublicationYear = table.Column<int>(type: "integer", nullable: false),
                    Genre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TotalCopies = table.Column<int>(type: "integer", nullable: false),
                    AvailableCopies = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Readers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RegistrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookLoans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    ReaderId = table.Column<int>(type: "integer", nullable: false),
                    LoanDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookLoans_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BookLoans_Readers_ReaderId",
                        column: x => x.ReaderId,
                        principalTable: "Readers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "Author", "AvailableCopies", "Genre", "ISBN", "PublicationYear", "Title", "TotalCopies" },
                values: new object[,]
                {
                    { 1, "Лев Толстой", 5, "Роман", "978-5-17-090000-1", 1869, "Война и мир", 5 },
                    { 2, "Фёдор Достоевский", 3, "Роман", "978-5-17-090000-2", 1866, "Преступление и наказание", 3 },
                    { 3, "Михаил Булгаков", 4, "Роман", "978-5-17-090000-3", 1967, "Мастер и Маргарита", 4 },
                    { 4, "Александр Пушкин", 2, "Поэма", "978-5-17-090000-4", 1833, "Евгений Онегин", 2 },
                    { 5, "Лев Толстой", 3, "Роман", "978-5-17-090000-5", 1877, "Анна Каренина", 3 }
                });

            migrationBuilder.InsertData(
                table: "Readers",
                columns: new[] { "Id", "Email", "FullName", "Phone", "RegistrationDate" },
                values: new object[,]
                {
                    { 1, "ivanov@example.com", "Иванов Иван Иванович", "+7-900-111-1111", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "petrova@example.com", "Петрова Мария Сергеевна", "+7-900-222-2222", new DateTime(2024, 2, 20, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "sidorov@example.com", "Сидоров Алексей Петрович", "+7-900-333-3333", new DateTime(2024, 3, 10, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookLoans_BookId",
                table: "BookLoans",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookLoans_ReaderId",
                table: "BookLoans",
                column: "ReaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_ISBN",
                table: "Books",
                column: "ISBN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Readers_Email",
                table: "Readers",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookLoans");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Readers");
        }
    }
}
