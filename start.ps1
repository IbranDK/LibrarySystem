# Скрипт запуска системы управления библиотекой
# Использование: .\start.ps1

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Система управления библиотекой" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Проверяем что Docker запущен
Write-Host "[1/6] Проверка Docker..." -ForegroundColor Yellow
$dockerRunning = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ОШИБКА: Docker не запущен!" -ForegroundColor Red
    Write-Host "Запустите Docker Desktop и попробуйте снова." -ForegroundColor Red
    exit 1
}
Write-Host "  Docker работает" -ForegroundColor Green

# Останавливаем старые контейнеры
Write-Host "[2/6] Остановка старых контейнеров..." -ForegroundColor Yellow
docker compose down 2>$null
Write-Host "  Готово" -ForegroundColor Green

# Запускаем контейнеры
Write-Host "[3/6] Запуск контейнеров..." -ForegroundColor Yellow
docker compose up -d --build
if ($LASTEXITCODE -ne 0) {
    Write-Host "ОШИБКА: Не удалось запустить контейнеры!" -ForegroundColor Red
    exit 1
}
Write-Host "  Контейнеры запущены" -ForegroundColor Green

# Ждём запуска
Write-Host "[4/6] Ожидание готовности сервисов..." -ForegroundColor Yellow
$maxRetries = 30
$retry = 0
do {
    Start-Sleep -Seconds 2
    $retry++
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/api/books" -UseBasicParsing -TimeoutSec 5 2>$null
        if ($response.StatusCode -eq 200) {
            break
        }
    } catch {
        Write-Host "  Ожидание API... ($retry/$maxRetries)" -ForegroundColor Gray
    }
} while ($retry -lt $maxRetries)

if ($retry -ge $maxRetries) {
    Write-Host "ОШИБКА: API не отвечает!" -ForegroundColor Red
    Write-Host "Проверьте логи: docker logs library-api" -ForegroundColor Yellow
    exit 1
}
Write-Host "  API готов" -ForegroundColor Green

# Настраиваем Nginx
Write-Host "[5/6] Настройка Nginx..." -ForegroundColor Yellow
docker exec library-nginx rm -f /etc/nginx/conf.d/default.conf 2>$null
docker cp nginx/nginx.conf library-nginx:/etc/nginx/nginx.conf
docker cp frontend/index.html library-nginx:/usr/share/nginx/html/index.html
docker cp frontend/app.js library-nginx:/usr/share/nginx/html/app.js
docker exec library-nginx nginx -s reload
Write-Host "  Nginx настроен" -ForegroundColor Green

# Проверяем что всё работает
Write-Host "[6/6] Финальная проверка..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
try {
    $response = Invoke-WebRequest -Uri "http://localhost:8080/api/books" -UseBasicParsing -TimeoutSec 5
    if ($response.StatusCode -eq 200) {
        Write-Host "  Всё работает!" -ForegroundColor Green
    }
} catch {
    Write-Host "  ВНИМАНИЕ: Проверьте http://localhost:8080 вручную" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Система запущена!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Адреса:" -ForegroundColor White
Write-Host "  Веб-интерфейс:  http://localhost:8080" -ForegroundColor Cyan
Write-Host "  API напрямую:   http://localhost:5000/api/books" -ForegroundColor Cyan
Write-Host "  Prometheus:     http://localhost:9090" -ForegroundColor Cyan
Write-Host "  Grafana:        http://localhost:3000 (admin/admin)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Для остановки: docker compose down" -ForegroundColor Gray
Write-Host ""