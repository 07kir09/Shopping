# Shopping Microservices

Проект представляет собой микросервисную архитектуру для системы онлайн-покупок, состоящую из двух основных сервисов:

## Сервисы

### Orders Service
- Управление заказами
- Создание и отслеживание статуса заказов
- Интеграция с платежной системой

### Payments Service
- Управление платежами
- Обработка транзакций
- Управление балансом пользователей

## Технологии

- .NET 8
- PostgreSQL
- RabbitMQ
- Docker
- Entity Framework Core

## Запуск проекта

1. Запустите Docker контейнеры:
```bash
docker-compose up -d
```

2. Запустите Orders Service:
```bash
cd src/Shopping.OrdersService
dotnet run
```

3. Запустите Payments Service:
```bash
cd src/Shopping.PaymentsService
dotnet run
```

## API Documentation

После запуска сервисов, документация API доступна через Swagger UI:
- Orders Service: http://localhost:5001/swagger
- Payments Service: http://localhost:5002/swagger

## Требования

- .NET 8.0 SDK
- Docker и Docker Compose
- SQL Server (можно использовать Docker)
- RabbitMQ (можно использовать Docker)

## Структура проекта

```
Shopping/
├── src/
│   ├── Shopping.Common/           # Общие компоненты
│   ├── Shopping.OrdersService/    # Сервис заказов
│   └── Shopping.PaymentsService/  # Сервис платежей
└── docker-compose.yml            # Конфигурация Docker
```

## Настройка базы данных

1. Создайте базы данных для сервисов:

```sql
CREATE DATABASE ShoppingOrders;
CREATE DATABASE ShoppingPayments;
```

2. Примените миграции (базы данных будут созданы автоматически при первом запуске сервисов)

## Запуск сервисов

1. Сделайте скрипт запуска исполняемым:

```bash
chmod +x run-services.sh
```

2. Запустите сервисы:

```bash
./run-services.sh
```

Сервисы будут доступны по следующим адресам:
- Orders Service: http://localhost:5001
- Payments Service: http://localhost:5002

## Тестирование API

### Swagger UI

Для удобного тестирования API используйте Swagger UI:
- Orders Service: http://localhost:5001/swagger
- Payments Service: http://localhost:5002/swagger

### Примеры запросов через curl

1. Создание заказа:
```bash
curl -X POST http://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "amount": 100.00,
    "description": "Test order"
  }'
```

2. Получение заказов пользователя:
```bash
curl http://localhost:5001/api/orders/user/123e4567-e89b-12d3-a456-426614174000
```

3. Создание платежного аккаунта:
```bash
curl -X POST http://localhost:5002/api/payments/accounts \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "123e4567-e89b-12d3-a456-426614174000"
  }'
```

4. Обработка платежа:
```bash
curl -X POST http://localhost:5002/api/payments/process \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "amount": 100.00,
    "orderId": "123e4567-e89b-12d3-a456-426614174001"
  }'
```

## Мониторинг

- RabbitMQ Management UI: http://localhost:15672
  - Логин: guest
  - Пароль: guest

## Остановка сервисов

1. Остановите сервисы:
```bash
pkill -f "dotnet run"
```

2. Остановите инфраструктуру:
```bash
docker-compose down
```

## Устранение неполадок

1. Проверьте логи сервисов:
```bash
tail -f /var/log/syslog | grep "dotnet"
```

2. Проверьте статус Docker контейнеров:
```bash
docker-compose ps
```

3. Проверьте логи Docker контейнеров:
```bash
docker-compose logs
``` 