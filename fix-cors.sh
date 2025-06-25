#!/bin/bash

echo "🔧 Исправление проблем с CORS в Shopping приложении"
echo "================================================="

echo "1. Исправление прав доступа..."
chmod +x fix-cors.sh fix-permission.sh start.sh test-api.sh run-services.sh

echo "2. Перезапуск сервисов..."
docker-compose restart payments-service orders-service

echo "3. Ожидание запуска сервисов..."
sleep 10

echo "4. Проверка сервисов..."
echo "Payments Service:"
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://localhost:5002/swagger

echo "Orders Service:"
curl -s -o /dev/null -w "HTTP %{http_code}\n" http://localhost:5001/swagger

echo ""
echo "5. Тестирование API..."
echo "Тестирование получения аккаунта:"
HTTP_CODE=$(curl -s -w "%{http_code}" -o /tmp/test_response.json "http://localhost:5002/api/Payments/accounts/00000000-0000-0000-0000-000000000001")
echo "HTTP Status: $HTTP_CODE"
if [ "$HTTP_CODE" -eq 200 ]; then
    echo "✅ API работает корректно"
    cat /tmp/test_response.json | head -3
else
    echo "❌ Проблема с API"
fi

echo ""
echo "6. Тестирование пополнения баланса..."
HTTP_CODE=$(curl -s -w "%{http_code}" -o /tmp/topup_response.json -X POST "http://localhost:5002/api/Payments/topup" \
     -H "Content-Type: application/json" \
     -d '{"userId": "00000000-0000-0000-0000-000000000001", "amount": 50}')
echo "HTTP Status: $HTTP_CODE"
if [ "$HTTP_CODE" -eq 200 ]; then
    echo "✅ Пополнение работает"
    cat /tmp/topup_response.json | head -3
else
    echo "❌ Проблема с пополнением"
    cat /tmp/topup_response.json
fi

echo ""
echo "✅ Диагностика завершена!"
echo ""
echo "📝 Для дальнейшей диагностики:"
echo "  • Откройте http://localhost/debug.html"
echo "  • Проверьте консоль браузера (F12)"
echo "  • Убедитесь что сервисы запущены: docker-compose ps"
echo "  • Полное исправление: ./fix-permission.sh" 