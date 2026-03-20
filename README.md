# Library System
## Система управления библиотекой

Микросервисная система на ASP.NET Core (.NET 10) с контейнеризацией, кэшированием и мониторингом.

### Технологии

ASP.NET Core Web API, PostgreSQL 16, Redis 7, Nginx, Prometheus, Grafana, Docker Compose, GitHub Actions, xUnit

### Запуск

```bash
git clone https://github.com/ВАШ_ЛОГИН/library-system.git
cd library-system
docker compose up --build
```
### Адреса
Сервис	URL
* API через Nginx	http://localhost:8080
* API напрямую	http://localhost:5000
* Prometheus	http://localhost:9090
* Grafana	http://localhost:3000 (admin/admin)
### API эндпоинты
#### Книги

| Метод | URL | Описание | Кэш |
|-------|-----|----------|------|
| GET | /api/books | Все книги | ✅ |
| GET | /api/books/{id} | Книга по ID | ✅ |
| POST | /api/books | Создать книгу | — |
| PUT | /api/books/{id} | Обновить книгу | — |
| DELETE | /api/books/{id} | Удалить книгу | — |

#### Читатели

| Метод | URL | Описание | Кэш |
|-------|-----|----------|------|
| GET | /api/readers | Все читатели | ✅ |
| GET | /api/readers/{id} | Читатель по ID | ✅ |
| POST | /api/readers | Создать читателя | — |
| PUT | /api/readers/{id} | Обновить читателя | — |
| DELETE | /api/readers/{id} | Удалить читателя | — |

#### Выдачи

| Метод | URL | Описание |
|-------|-----|----------|
| GET | /api/bookloans | Все выдачи |
| GET | /api/bookloans/{id} | Выдача по ID |
| POST | /api/bookloans | Выдать книгу |
| PUT | /api/bookloans/{id}/return | Вернуть книгу |
| DELETE | /api/bookloans/{id} | Удалить запись |
### Клиент
```bash
cd src/LibraryClient
dotnet run -- http://localhost:8080
Или
.\start.ps1 - скрипт запуска графического интерфейса. Графический интерфейс: http://localhost:8080/
Скрипт остановки:
.\stop.ps1
```

### Тесты
```bash
dotnet test
