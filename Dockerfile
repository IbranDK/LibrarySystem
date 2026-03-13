# ============================================================
# ЭТАП 1: СБОРКА (build stage)
# ============================================================
# Используем полный SDK-образ (содержит компилятор, NuGet и т.д.)
# Этот образ весит ~800MB, но нам нужен только для сборки
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Сначала копируем только файл проекта и восстанавливаем зависимости.
# Это отдельный шаг, потому что Docker кэширует каждый слой.
# Если .csproj не изменился, Docker возьмёт пакеты из кэша (быстрее).
COPY src/LibraryApi/LibraryApi.csproj src/LibraryApi/
RUN dotnet restore src/LibraryApi/LibraryApi.csproj

# Теперь копируем весь исходный код и собираем приложение
COPY src/LibraryApi/ src/LibraryApi/
RUN dotnet publish src/LibraryApi/LibraryApi.csproj \
    -c Release \
    -o /app/publish

# ============================================================
# ЭТАП 2: RUNTIME (runtime stage)
# ============================================================
# Используем лёгкий runtime-образ (содержит только .NET Runtime, ~200MB)
# Итоговый образ НЕ содержит исходный код, SDK и промежуточные файлы
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app

# Копируем только скомпилированные файлы из этапа build
COPY --from=build /app/publish .

# Указываем, на каком порту будет слушать приложение
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

# Команда запуска приложения
ENTRYPOINT ["dotnet", "LibraryApi.dll"]