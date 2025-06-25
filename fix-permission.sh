#!/bin/bash

echo "🔧 Исправление прав доступа и настройка Shopping приложения"
echo "=========================================================="

# Исправление прав доступа
echo "1. Исправление прав доступа к скриптам..."
chmod +x fix-cors.sh
chmod +x start.sh
chmod +x test-api.sh
chmod +x run-services.sh

echo "2. Перезапуск сервисов..."
docker-compose restart payments-service orders-service

echo "3. Ожидание запуска сервисов..."
sleep 10

echo "4. Создание тестовых аккаунтов..."

# Создание аккаунта для пользователя 1
echo "Создание аккаунта 1..."
curl -s -X POST "http://localhost:5002/api/Payments/accounts" \
     -H "Content-Type: application/json" \
     -d '{"userId": "00000000-0000-0000-0000-000000000001"}' | head -5

echo ""

# Создание аккаунта для пользователя 2  
echo "Создание аккаунта 2..."
curl -s -X POST "http://localhost:5002/api/Payments/accounts" \
     -H "Content-Type: application/json" \
     -d '{"userId": "00000000-0000-0000-0000-000000000002"}' | head -5

echo ""

# Пополнение балансов
echo "5. Пополнение балансов..."
curl -s -X POST "http://localhost:5002/api/Payments/topup" \
     -H "Content-Type: application/json" \
     -d '{"userId": "00000000-0000-0000-0000-000000000001", "amount": 1000}' | head -5

echo ""

curl -s -X POST "http://localhost:5002/api/Payments/topup" \
     -H "Content-Type: application/json" \
     -d '{"userId": "00000000-0000-0000-0000-000000000002", "amount": 500}' | head -5

echo ""
echo "6. Проверка создания аккаунтов..."
echo "Аккаунт 1:"
curl -s "http://localhost:5002/api/Payments/accounts/00000000-0000-0000-0000-000000000001" | head -3

echo ""
echo "Аккаунт 2:"  
curl -s "http://localhost:5002/api/Payments/accounts/00000000-0000-0000-0000-000000000002" | head -3

echo ""
echo "✅ Настройка завершена!"
echo ""
echo "📱 Теперь можно использовать:"
echo "  • http://localhost - веб-интерфейс"
echo "  • http://localhost/debug.html - диагностика"
echo "  • make fix - исправление проблем"
echo "  • make help - все команды" 