# Shopping Microservices

A microservices-based shopping application built with .NET 8, PostgreSQL, and RabbitMQ.

## Architecture

The application consists of two main microservices:

1. **Orders Service** (Port 5001)
   - Manages order creation and tracking
   - Uses PostgreSQL for data storage
   - Communicates with Payments Service via RabbitMQ

2. **Payments Service** (Port 5002)
   - Handles payment processing
   - Uses PostgreSQL for data storage
   - Communicates with Orders Service via RabbitMQ

## Prerequisites

- .NET 8 SDK
- Docker and Docker Compose
- Git

## Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/shopping.git
   cd shopping
   ```

2. Start the infrastructure (PostgreSQL and RabbitMQ):
   ```bash
   docker-compose up -d
   ```

3. Start the services:
   ```bash
   ./run-services.sh
   ```

4. Access the Swagger UI:
   - Orders Service: http://localhost:5001/swagger
   - Payments Service: http://localhost:5002/swagger

## API Documentation

### Orders Service

- `POST /api/orders` - Create a new order
- `GET /api/orders/user/{userId}` - Get user's orders

### Payments Service

- `POST /api/payments/accounts` - Create a payment account
- `POST /api/payments/process` - Process a payment

## Development

The solution is structured as follows:

```
src/
├── Shopping.Common/           # Shared code and interfaces
├── Shopping.OrdersService/    # Orders microservice
└── Shopping.PaymentsService/  # Payments microservice
```

## License

MIT 