#!/bin/bash

echo "🚀 Запуск Shopping - Микросервисное приложение интернет-магазина"
echo "================================================================="
echo ""

# Проверка наличия Docker
if ! command -v docker &> /dev/null; then
    echo "❌ Docker не установлен. Установите Docker и попробуйте снова."
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose не установлен. Установите Docker Compose и попробуйте снова."
    exit 1
fi

echo "✅ Docker и Docker Compose найдены"
echo ""

# Проверка занятых портов
echo "🔍 Проверка портов..."
if lsof -Pi :80 -sTCP:LISTEN -t >/dev/null ; then
    echo "⚠️  Порт 80 занят. Остановите сервис на порту 80 или измените конфигурацию."
fi

if lsof -Pi :5001 -sTCP:LISTEN -t >/dev/null ; then
    echo "⚠️  Порт 5001 занят. Остановите сервис на порту 5001 или измените конфигурацию."
fi

if lsof -Pi :5002 -sTCP:LISTEN -t >/dev/null ; then
    echo "⚠️  Порт 5002 занят. Остановите сервис на порту 5002 или измените конфигурацию."
fi

echo "📦 Запуск сервисов..."
docker-compose up -d --build

echo ""
echo "⏳ Ожидание запуска сервисов..."
sleep 10

echo ""
echo "📊 Статус контейнеров:"
docker-compose ps

echo ""
echo "🎉 Приложение запущено!"
echo ""
echo "📱 Доступные URL:"
echo "  • Веб-интерфейс:     http://localhost"
echo "  • Orders API:        http://localhost:5001/swagger"
echo "  • Payments API:      http://localhost:5002/swagger"  
echo "  • RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "  • 🔧 Диагностика:     http://localhost/debug.html"
echo ""
echo "📝 Полезные команды:"
echo "  • Просмотр логов:    docker-compose logs -f"
echo "  • Остановка:         docker-compose down"
echo "  • Перезапуск:        docker-compose restart"
echo "  • Make команды:      make help"
echo ""
echo "🆘 Если возникли проблемы:"
echo "  • Быстрое исправление: ./fix-cors.sh или make fix"
echo "  • Диагностика:        http://localhost/debug.html"
echo "  • Подробное руководство: cat troubleshoot.md"
echo ""
echo "🔧 Для устранения проблем смотрите README.md" 