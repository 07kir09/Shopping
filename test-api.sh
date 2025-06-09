#!/bin/bash

# Цвета для вывода
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

echo "Testing Shopping API..."

# 1. Создание аккаунта
echo -e "\n${GREEN}1. Creating account...${NC}"
ACCOUNT_RESPONSE=$(curl -s -X POST http://localhost:5001/api/payments/accounts \
  -H "Content-Type: application/json" \
  -d '{"userId": "00000000-0000-0000-0000-000000000001"}')
echo "Response: $ACCOUNT_RESPONSE"

# 2. Создание заказа
echo -e "\n${GREEN}2. Creating order...${NC}"
ORDER_RESPONSE=$(curl -s -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "00000000-0000-0000-0000-000000000001",
    "amount": 100.00,
    "description": "Test order"
  }')
echo "Response: $ORDER_RESPONSE"

# Извлекаем ID заказа из ответа
ORDER_ID=$(echo $ORDER_RESPONSE | grep -o '"id":"[^"]*' | cut -d'"' -f4)

# 3. Проверка статуса заказа
echo -e "\n${GREEN}3. Checking order status...${NC}"
curl -s http://localhost:5000/api/orders/$ORDER_ID

# 4. Проверка баланса
echo -e "\n${GREEN}4. Checking account balance...${NC}"
curl -s http://localhost:5001/api/payments/accounts/00000000-0000-0000-0000-000000000001 