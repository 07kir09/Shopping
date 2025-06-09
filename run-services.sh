#!/bin/bash

# Start Orders Service
echo "Starting Orders Service..."
cd src/Shopping.OrdersService
dotnet run &
ORDERS_PID=$!

# Start Payments Service
echo "Starting Payments Service..."
cd ../Shopping.PaymentsService
dotnet run &
PAYMENTS_PID=$!

echo "Services are running!"
echo "Orders Service PID: $ORDERS_PID"
echo "Payments Service PID: $PAYMENTS_PID"

# Wait for user to press Ctrl+C
trap "kill $ORDERS_PID $PAYMENTS_PID; exit" INT
wait 