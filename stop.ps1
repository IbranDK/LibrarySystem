# Скрипт остановки системы
Write-Host ""
Write-Host "Остановка системы..." -ForegroundColor Yellow
docker compose down
Write-Host "Система остановлена." -ForegroundColor Green
Write-Host ""