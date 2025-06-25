# Shopping - Микросервисное приложение интернет-магазина

**Автор:** ВОЯКИН КИРИЛЛ ВЛАДИСЛАВОВИЧ

## Описание проекта

Микросервисное приложение для управления заказами и платежами в интернет-магазине. Система реализует паттерн SAGA для обеспечения согласованности данных между сервисами.

## Архитектура системы

### Компоненты

1. **Orders Service** - Сервис управления заказами
2. **Payments Service** - Сервис управления платежами и аккаунтами
3. **Web Interface** - Веб-интерфейс для взаимодействия с API
4. **PostgreSQL** - Реляционная база данных
5. **RabbitMQ** - Брокер сообщений для асинхронной коммуникации

### Технологический стек

- **.NET 8.0** - Платформа разработки
- **ASP.NET Core** - Веб-фреймворк
- **Entity Framework Core** - ORM
- **PostgreSQL** - СУБД
- **RabbitMQ** - Брокер сообщений
- **Docker** - Контейнеризация
- **Swagger/OpenAPI** - Документация API
- **HTML/CSS/JavaScript** - Фронтенд

## Функциональные требования

### Orders Service

#### Endpoints

| Метод | URL | Описание |
|-------|-----|----------|
| POST | /api/Orders | Создание заказа с автоматическим списанием средств |
| GET | /api/Orders/user/{userId} | Получение заказов пользователя |
| GET | /api/Orders/{orderId} | Получение заказа по ID |
| PUT | /api/Orders/{orderId}/status | Обновление статуса заказа |

#### Бизнес-логика

1. При создании заказа система проверяет наличие аккаунта пользователя
2. Проверяется достаточность средств на счету
3. При успешной валидации происходит списание средств через Payments Service
4. Заказ создается со статусом "Paid" только после успешного списания
5. При недостатке средств заказ отклоняется

### Payments Service

#### Endpoints

| Метод | URL | Описание |
|-------|-----|----------|
| GET | /api/Payments/accounts/{userId} | Получение информации об аккаунте |
| POST | /api/Payments/accounts | Создание нового аккаунта |
| POST | /api/Payments/topup | Пополнение баланса аккаунта |
| POST | /api/Payments/process | Обработка платежа (списание средств) |
| POST | /api/Payments/refund | Возврат средств |

#### Бизнес-логика

1. Каждый пользователь имеет один аккаунт с балансом
2. Все операции с балансом выполняются атомарно
3. Ведется аудит всех операций через Outbox Pattern
4. Поддерживается возврат средств при отмене заказов

## Модель данных

### Orders Service

#### Таблица Orders
```sql
CREATE TABLE "Orders" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID NOT NULL,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Description" TEXT NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL
);
```

#### Таблица OutboxMessages
```sql
CREATE TABLE "OutboxMessages" (
    "Id" UUID PRIMARY KEY,
    "MessageType" VARCHAR(100) NOT NULL,
    "Payload" TEXT NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "ProcessedAt" TIMESTAMP NULL
);
```

### Payments Service

#### Таблица Accounts
```sql
CREATE TABLE "Accounts" (
    "Id" UUID PRIMARY KEY,
    "UserId" UUID UNIQUE NOT NULL,
    "Balance" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL
);
```

#### Таблица OutboxMessages
```sql
CREATE TABLE "OutboxMessages" (
    "Id" UUID PRIMARY KEY,
    "MessageType" VARCHAR(100) NOT NULL,
    "Payload" TEXT NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "ProcessedAt" TIMESTAMP NULL
);
```

## Паттерны проектирования

### Outbox Pattern
Используется для обеспечения согласованности между изменениями в базе данных и публикацией событий в RabbitMQ.

### SAGA Pattern
Реализован хореографический подход для координации транзакций между сервисами.

### Repository Pattern
Инкапсуляция логики доступа к данным через Entity Framework Core.

## Конфигурация развертывания

### Docker Compose

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:latest
    environment:
      POSTGRES_DB: shopping
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"

  orders-service:
    build: ./src/Shopping.OrdersService
    ports:
      - "5001:80"
    depends_on:
      - postgres
      - rabbitmq

  payments-service:
    build: ./src/Shopping.PaymentsService
    ports:
      - "5002:80"
    depends_on:
      - postgres
      - rabbitmq

  web:
    build: ./src/shopping-web
    ports:
      - "80:80"
    depends_on:
      - orders-service
      - payments-service
```

## Запуск проекта

### Требования
- Docker 20.0+
- Docker Compose 2.0+

### 🚀 Быстрый старт (одна команда)

Для мгновенного запуска всего проекта:

**Вариант 1: Bash скрипт**
```bash
# Скачайте проект и перейдите в директорию
cd Shopping

# Запустите автоматический скрипт
./start.sh
```

**Вариант 2: Make команды**
```bash
# Скачайте проект и перейдите в директорию
cd Shopping

# Запустите через Make
make start

# Или посмотрите все доступные команды
make help

# Исправление проблем одной командой
make fix

# Диагностика
make debug
```

Скрипт автоматически:
- Проверит наличие Docker
- Проверит свободные порты
- Запустит все сервисы
- Покажет статус и доступные URL

### Инструкция для первого запуска

После клонирования/скачивания проекта выполните следующие шаги:

#### 1. Подготовка окружения
```bash
# Убедитесь, что Docker запущен
docker --version
docker-compose --version

# Перейдите в корневую директорию проекта
cd Shopping
```

#### 2. Первый запуск (автоматическое создание таблиц)
```bash
# Запуск всех сервисов с пересборкой
docker-compose up -d --build

# Проверка статуса всех контейнеров
docker-compose ps

# Просмотр логов для проверки корректного запуска
docker-compose logs -f
```

#### 3. Проверка работоспособности
```bash
# Проверка Orders Service
curl http://localhost:5001/swagger

# Проверка Payments Service  
curl http://localhost:5002/swagger

# Проверка веб-интерфейса
curl http://localhost
```

#### 4. Создание тестового аккаунта (опционально)
```bash
# Создание аккаунта через API
curl -X POST "http://localhost:5002/api/Payments/accounts" \
     -H "Content-Type: application/json" \
     -d '{"userId": "00000000-0000-0000-0000-000000000001"}'
```

### Команды

```bash
# Запуск всех сервисов
docker-compose up -d

# Запуск с пересборкой
docker-compose up -d --build

# Остановка всех сервисов
docker-compose down

# Полная очистка (с удалением volumes)
docker-compose down -v

# Просмотр логов
docker-compose logs -f

# Проверка статуса
docker-compose ps

# Перезапуск конкретного сервиса
docker-compose restart orders-service
```

### Устранение проблем

#### Ошибка 500 Internal Server Error при работе с аккаунтами

**Симптомы:**
```
[Error] Failed to load resource: the server responded with a status of 500 (Internal Server Error)
[Error] Error getting account: – SyntaxError: The string did not match the expected pattern.
```

**Причина:** CORS (Cross-Origin Resource Sharing) проблемы или несовместимость конфигурации.

**Решение:**

1. **Быстрое исправление:**
   ```bash
   # Запустите скрипт диагностики и исправления
   ./fix-cors.sh
   ```

2. **Ручное исправление:**
   ```bash
   # Перезапустите сервисы
   docker-compose restart payments-service orders-service
   
   # Проверьте статус
   docker-compose ps
   
   # Проверьте доступность API
   curl http://localhost:5002/api/Payments/accounts/00000000-0000-0000-0000-000000000001
   ```

3. **Диагностика через браузер:**
   - Откройте http://localhost/debug.html
   - Нажмите "Проверить сервисы"
   - Проверьте консоль браузера (F12)

4. **Если проблема продолжается:**
   ```bash
   # Полная пересборка
   docker-compose down
   docker-compose up -d --build
   
   # Очистка данных (ВНИМАНИЕ: удалит все данные)
   docker-compose down -v
   docker-compose up -d --build
   ```

#### Если контейнеры не запускаются:
```bash
# Проверьте, свободны ли порты
netstat -tulpn | grep :80
netstat -tulpn | grep :5001
netstat -tulpn | grep :5002

# Пересоберите образы
docker-compose build --no-cache

# Очистите старые образы
docker system prune -f
```

#### Если базы данных не создаются:
```bash
# Пересоздайте volume PostgreSQL
docker-compose down -v
docker volume rm shopping_postgres_data
docker-compose up -d
```

#### Дополнительные инструменты диагностики

- **debug.html** - http://localhost/debug.html (интерактивная диагностика)
- **fix-cors.sh** - автоматическое исправление CORS проблем
- **test-api.sh** - тестирование API через командную строку

📚 **Подробное руководство по устранению неполадок:** [troubleshoot.md](troubleshoot.md)

## Доступные URL

| Сервис | URL | Описание |
|--------|-----|----------|
| Web Interface | http://localhost:80 | Веб-интерфейс приложения |
| Orders API | http://localhost:5001/swagger | Документация Orders Service |
| Payments API | http://localhost:5002/swagger | Документация Payments Service |
| RabbitMQ Management | http://localhost:15672 | Управление RabbitMQ |
| PostgreSQL | localhost:5432 | База данных |

### Учетные данные

- **RabbitMQ:** guest/guest
- **PostgreSQL:** postgres/postgres

## Тестирование

### Тестовые аккаунты

```
ID: 00000000-0000-0000-0000-000000000001 (баланс: переменный)
ID: 00000000-0000-0000-0000-000000000002 (баланс: 0)
ID: 00000000-0000-0000-0000-000000000003 (баланс: переменный)
```

### Сценарии тестирования

1. **Создание аккаунта**
   - POST /api/Payments/accounts
   - Проверка создания записи в БД

2. **Пополнение баланса**
   - POST /api/Payments/topup
   - Проверка увеличения баланса

3. **Создание заказа с достаточными средствами**
   - POST /api/Orders
   - Проверка списания средств
   - Проверка статуса заказа "Paid"

4. **Создание заказа с недостаточными средствами**
   - POST /api/Orders с суммой больше баланса
   - Проверка отклонения заказа
   - Проверка неизменности баланса

## Мониторинг и логирование

### Health Checks
- PostgreSQL: pg_isready
- RabbitMQ: rabbitmq-diagnostics ping

### Логирование
- Структурированные логи в формате JSON
- Уровни: Information, Warning, Error
- Трассировка запросов между сервисами

## Безопасность

### Валидация входных данных
- Проверка формата GUID для идентификаторов пользователей
- Валидация сумм платежей (положительные значения)
- Санитизация пользовательского ввода

### Обработка ошибок
- Graceful degradation при недоступности зависимых сервисов
- Retry механизмы для временных сбоев
- Детальные коды ошибок в API responses
