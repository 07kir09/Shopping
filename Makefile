.PHONY: help start stop restart logs status clean build test dev debug fix troubleshoot

# Цвета для вывода
YELLOW := \033[1;33m
GREEN := \033[1;32m
RED := \033[1;31m
NC := \033[0m # No Color

help: ## Показать справку
	@echo "$(YELLOW)Shopping - Микросервисное приложение$(NC)"
	@echo ""
	@echo "$(GREEN)Доступные команды:$(NC)"
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  $(GREEN)%-12s$(NC) %s\n", $$1, $$2}' $(MAKEFILE_LIST)

start: ## Запустить все сервисы
	@echo "$(YELLOW)🚀 Запуск всех сервисов...$(NC)"
	docker-compose up -d --build
	@echo "$(GREEN)✅ Сервисы запущены!$(NC)"
	@make status

stop: ## Остановить все сервисы
	@echo "$(YELLOW)🛑 Остановка всех сервисов...$(NC)"
	docker-compose down
	@echo "$(GREEN)✅ Сервисы остановлены!$(NC)"

restart: ## Перезапустить все сервисы
	@echo "$(YELLOW)🔄 Перезапуск всех сервисов...$(NC)"
	docker-compose down
	docker-compose up -d --build
	@echo "$(GREEN)✅ Сервисы перезапущены!$(NC)"

logs: ## Показать логи всех сервисов
	docker-compose logs -f

status: ## Показать статус всех контейнеров
	@echo "$(YELLOW)📊 Статус контейнеров:$(NC)"
	docker-compose ps
	@echo ""
	@echo "$(GREEN)📱 Доступные URL:$(NC)"
	@echo "  • Веб-интерфейс:       http://localhost"
	@echo "  • Orders API:          http://localhost:5001/swagger"
	@echo "  • Payments API:        http://localhost:5002/swagger"
	@echo "  • RabbitMQ Management: http://localhost:15672"

clean: ## Полная очистка (контейнеры + volumes)
	@echo "$(RED)🧹 Полная очистка проекта...$(NC)"
	docker-compose down -v
	docker system prune -f
	@echo "$(GREEN)✅ Очистка завершена!$(NC)"

build: ## Пересобрать все образы
	@echo "$(YELLOW)🔨 Пересборка всех образов...$(NC)"
	docker-compose build --no-cache
	@echo "$(GREEN)✅ Образы пересобраны!$(NC)"

test: ## Запустить тесты
	@echo "$(YELLOW)🧪 Запуск тестов...$(NC)"
	cd src/Shopping.OrdersService.Tests && dotnet test
	cd src/Shopping.PaymentsService.Tests && dotnet test
	cd src/Shopping.Common.Tests && dotnet test
	@echo "$(GREEN)✅ Тесты завершены!$(NC)"

dev: ## Запуск в режиме разработки (без build)
	docker-compose up -d
	@make status

debug: ## Открыть диагностическую страницу
	@echo "$(YELLOW)🔍 Запуск диагностики...$(NC)"
	@echo "Откройте в браузере: http://localhost/debug.html"
	@echo "Или выполните: ./fix-cors.sh"

fix: ## Исправить проблемы с CORS
	@echo "$(YELLOW)🔧 Исправление проблем с CORS...$(NC)"
	bash ./fix-cors.sh

fix-all: ## Полное исправление всех проблем (права доступа + настройка)
	@echo "$(YELLOW)🔧 Полное исправление всех проблем...$(NC)"
	bash ./fix-permission.sh

troubleshoot: ## Показать справку по устранению неполадок
	@echo "$(YELLOW)📚 Частые проблемы и решения:$(NC)"
	@echo ""
	@echo "$(GREEN)1. Ошибка 500 (CORS):$(NC)"
	@echo "   make fix"
	@echo ""
	@echo "$(GREEN)2. Контейнеры не запускаются:$(NC)"
	@echo "   make clean && make start"
	@echo ""
	@echo "$(GREEN)3. База данных не создается:$(NC)"
	@echo "   docker-compose down -v && make start"
	@echo ""
	@echo "$(GREEN)4. Интерактивная диагностика:$(NC)"
	@echo "   make debug"
	@echo ""
	@echo "📖 Полное руководство: cat troubleshoot.md" 