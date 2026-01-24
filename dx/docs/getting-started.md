# Getting Started with Pet Shop API

Welcome! This guide will help you get up and running with the Pet Shop API in minutes.

## Prerequisites

Before you begin, ensure you have:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
- A code editor (Visual Studio, VS Code, Rider, etc.)
- Basic familiarity with REST APIs and JSON

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd PetShop
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build PetShop.slnx
```

### 4. Run the API

```bash
dotnet run --project PetShop.Api
```

The API will start on `http://localhost:7052`

### 5. Access Swagger UI

Open your browser and navigate to:

```
http://localhost:7052/swagger
```

Swagger UI provides an interactive interface to explore and test all API endpoints.

## Your First API Call

Let's create a customer and an order to see the API in action.

### Step 1: Create a Customer

**Request:**
```bash
POST http://localhost:7052/api/v1/customers
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "555-0123"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "555-0123",
  "estimatedPaymentDue": 0.00,
  "actualPaymentDue": 0.00
}
```

### Step 2: Create an Order

**Request:**
```bash
POST http://localhost:7052/api/v1/orders
Content-Type: application/json

{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "pickupDate": "2026-02-01"
}
```

**Response:**
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Open",
  "pickupDate": "2026-02-01T00:00:00",
  "pets": [],
  "estimatedCost": 0.00,
  "actualCost": null
}
```

### Step 3: Add a Pet to the Order

**Request:**
```bash
POST http://localhost:7052/api/v1/orders/7c9e6679-7425-40de-944b-e07fc1f90ae7/pets
Content-Type: application/json

{
  "name": "Fluffy",
  "kind": "Cat",
  "color": "White",
  "price": 299.99
}
```

**Response:**
```json
{
  "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
  "name": "Fluffy",
  "kind": "Cat",
  "color": "White",
  "price": 299.99
}
```

### Step 4: View the Order

**Request:**
```bash
GET http://localhost:7052/api/v1/orders/7c9e6679-7425-40de-944b-e07fc1f90ae7
```

**Response:**
```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Open",
  "pickupDate": "2026-02-01T00:00:00",
  "pets": [
    {
      "id": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d",
      "name": "Fluffy",
      "kind": "Cat",
      "color": "White",
      "price": 299.99
    }
  ],
  "estimatedCost": 299.99,
  "actualCost": null
}
```

Notice that `estimatedCost` is automatically calculated as the sum of pet prices.

## Using Docker

If you prefer Docker, you can run the API in a container:

```bash
# Build and start
docker-compose up -d --build

# View logs
docker-compose logs -f petshop-api

# Stop
docker-compose down
```

The API will be available at the same URL: `http://localhost:7052`

## Testing with cURL

Here are some example cURL commands:

```bash
# Create a customer
curl -X POST http://localhost:7052/api/v1/customers \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Jane","lastName":"Smith","email":"jane@example.com"}'

# Get all customers
curl http://localhost:7052/api/v1/customers

# Create an order
curl -X POST http://localhost:7052/api/v1/orders \
  -H "Content-Type: application/json" \
  -d '{"customerId":"<customer-id>","pickupDate":"2026-02-01"}'
```

## Testing with Postman

1. Import the OpenAPI spec from Swagger UI
2. Set the base URL to `http://localhost:7052/api/v1`
3. Start making requests!

## Next Steps

Now that you have the API running:

1. **Explore the API**: Use Swagger UI to try different endpoints
2. **Read the API Reference**: See [API Reference](api-reference.md) for complete endpoint documentation
3. **Understand Orders**: Learn about order states in [Order Lifecycle](order-lifecycle.md)
4. **Review Architecture**: Understand the design in [Architecture](architecture.md)

## Common Issues

### Port Already in Use

If port 7052 is already in use, you can change it:

1. Edit `PetShop.Api/Properties/launchSettings.json`
2. Change the `applicationUrl` to use a different port
3. Restart the API

### Database Warnings

The API uses an in-memory database for development. Data is lost when the application restarts. This is expected behavior for development and testing.

For production deployment, see the deployment documentation.

## Need Help?

- Check the [API Reference](api-reference.md) for detailed endpoint information
- Review [Order Lifecycle](order-lifecycle.md) for order state management
- Explore the Swagger UI for interactive API exploration

Happy coding! 🚀
