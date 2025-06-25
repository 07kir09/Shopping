# 🔧 Устранение неполадок Shopping приложения

## Частые проблемы и их решения

### ❌ Ошибка 500 "SyntaxError: The string did not match the expected pattern"

**Проблема:** Веб-интерфейс не может получить данные аккаунта, показывает ошибку в консоли.

**Диагностика:**
1. Откройте http://localhost/debug.html
2. Нажмите "Проверить сервисы"
3. Попробуйте "Получить аккаунт"

**Решение:**
```bash
# Автоматическое исправление
./fix-cors.sh

# Или вручную:
docker-compose restart payments-service orders-service
sleep 5
curl http://localhost:5002/api/Payments/accounts/00000000-0000-0000-0000-000000000001
```

### ❌ Контейнеры не запускаются

**Симптомы:**
- `docker-compose ps` показывает "Exited"
- Ошибки в `docker-compose logs`

**Решение:**
```bash
# 1. Проверьте порты
netstat -tulpn | grep -E ':(80|5001|5002|5432|5672)'

# 2. Освободите порты (если заняты)
sudo lsof -ti:80 | xargs kill -9  # Будьте осторожны!

# 3. Пересоберите образы
docker-compose down
docker-compose build --no-cache
docker-compose up -d

# 4. Проверьте логи
docker-compose logs -f
```

### ❌ База данных не создается

**Симптомы:**
- "Connection failed" в логах
- Таблицы не создаются

**Решение:**
```bash
# Полная очистка и пересоздание
docker-compose down -v
docker volume ls | grep shopping
docker volume rm shopping_postgres_data shopping_rabbitmq_data
docker-compose up -d --build
```

### ❌ CORS ошибки в браузере

**Симптомы:**
- "CORS policy" в консоли браузера
- Запросы блокируются

**Решение:**
1. Убедитесь что сервисы перезапущены после изменений
2. Проверьте что в Program.cs правильно настроен CORS:
   ```csharp
   app.UseCors("AllowAll");
   ```
3. Используйте debug.html для тестирования

### ❌ Аккаунт не найден (404)

**Проблема:** При попытке создать заказ система не находит аккаунт пользователя.

**Решение:**
```bash
# Создайте аккаунт через API
curl -X POST "http://localhost:5002/api/Payments/accounts" \
     -H "Content-Type: application/json" \
     -d '{"userId": "00000000-0000-0000-0000-000000000001"}'

# Или через веб-интерфейс:
# 1. Попробуйте создать заказ
# 2. Система автоматически создаст аккаунт
```

### ❌ Недостаточно средств

**Проблема:** "Insufficient funds" при создании заказа.

**Решение:**
```bash
# Пополните баланс через API
curl -X POST "http://localhost:5002/api/Payments/topup" \
     -H "Content-Type: application/json" \
     -d '{"userId": "00000000-0000-0000-0000-000000000001", "amount": 1000}'

# Или через веб-интерфейс в разделе "Аккаунт"
```

## Диагностические инструменты

### 1. debug.html
Интерактивная диагностика через браузер:
```
http://localhost/debug.html
```

### 2. fix-cors.sh
Автоматическое исправление CORS проблем:
```bash
./fix-cors.sh
```

### 3. test-api.sh
Тестирование API через командную строку:
```bash
./test-api.sh
```

### 4. Проверка логов
```bash
# Все сервисы
docker-compose logs -f

# Конкретный сервис
docker-compose logs -f payments-service
docker-compose logs -f orders-service
docker-compose logs -f postgres
```

### 5. Проверка состояния
```bash
# Статус контейнеров
docker-compose ps

# Использование ресурсов
docker stats

# Сетевые подключения
docker network ls
docker network inspect shopping_shopping-network
```

## Команды для экстренных случаев

### Полная перезагрузка
```bash
docker-compose down -v
docker system prune -a -f
docker-compose up -d --build
```

### Только пересборка образов
```bash
docker-compose build --no-cache
docker-compose up -d
```

### Сброс базы данных
```bash
docker-compose down
docker volume rm shopping_postgres_data
docker-compose up -d
```

## Полезные ссылки для диагностики

- **Swagger API Documentation:**
  - Orders: http://localhost:5001/swagger
  - Payments: http://localhost:5002/swagger

- **RabbitMQ Management:** http://localhost:15672 (guest/guest)

- **PostgreSQL:** localhost:5432 (postgres/postgres)

- **Диагностическая страница:** http://localhost/debug.html

## Если ничего не помогает

1. Создайте issue в репозитории с:
   - Выводом `docker-compose logs`
   - Выводом `docker-compose ps`
   - Описанием проблемы и шагов воспроизведения

2. Или обратитесь к разработчику:
   - **Автор:** ВОЯКИН КИРИЛЛ ВЛАДИСЛАВОВИЧ
   - **Email:** support@shopping.com 