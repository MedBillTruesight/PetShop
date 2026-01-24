# Pet Shop API

A production-ready RESTful API for managing a pet shop's customer orders and sales. Built with .NET 8.0 following Clean Architecture principles, this API provides a robust foundation for pet shop management systems.

## 🎯 Overview

The Pet Shop API is a comprehensive web service that handles customer information, order lifecycle management, and payment calculations. It's designed with production-grade standards, emphasizing maintainability, testability, and clear separation of concerns.

### Key Features

- ✅ **Customer Management**: Full CRUD operations for customer records with payment calculations
- ✅ **Order Lifecycle**: State machine-based order management (Open → Processing → Delivered)
- ✅ **Pet Management**: Add, remove, and manage pets within orders
- ✅ **Cost Calculation**: Automatic estimated vs. actual cost tracking
- ✅ **Business Rules**: Enforced domain rules for order state transitions
- ✅ **API Versioning**: URL-based versioning (`/api/v1/`) for contract stability
- ✅ **Comprehensive Testing**: Unit, integration, and API contract tests
- ✅ **Swagger Documentation**: Interactive API documentation with examples
- ✅ **Error Handling**: Structured error responses with appropriate HTTP status codes
- ✅ **Docker Support**: Containerized deployment with Docker Compose

## 🏗️ Architecture

The solution follows **Clean Architecture** principles with clear layer separation:

```
PetShop/
├── PetShop.Api/              # Presentation layer (HTTP endpoints, DTOs)
├── PetShop.Application/      # Application services (use cases, orchestration)
├── PetShop.Domain/           # Business logic (entities, rules, exceptions)
├── PetShop.Infrastructure/   # Data access (DbContext, repositories)
└── PetShop.Tests/            # Comprehensive test suite
```

### Design Principles

- **Domain-Driven Design**: Business logic encapsulated in domain models
- **Dependency Inversion**: Dependencies point inward toward the domain
- **Testability**: All business logic unit-testable without infrastructure
- **Immutability**: Order state becomes immutable after delivery
- **Fail-Fast Validation**: Input validation at API boundaries

For detailed architecture documentation, see [dx/docs/architecture.md](dx/docs/architecture.md).

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Docker and Docker Compose (optional, for containerized deployment)

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PetShop
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build PetShop.slnx
   ```

4. **Run the API**
   ```bash
   dotnet run --project PetShop.Api
   ```

5. **Access Swagger UI**
   Open your browser to: `http://localhost:7052/swagger`

### Docker Deployment

For quick deployment using Docker Compose:

```bash
# Build and start the API
docker-compose up -d --build

# View logs
docker-compose logs -f petshop-api

# Stop the API
docker-compose down
```

The API will be available at `http://localhost:7052/swagger`.

For detailed deployment instructions, see [README-DEPLOYMENT.md](README-DEPLOYMENT.md) and [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md).

## 📚 API Documentation

### Base URL

```
http://localhost:7052/api/v1
```

### Endpoints

#### Customers

- `GET /api/v1/customers` - List all customers
- `GET /api/v1/customers/{id}` - Get customer by ID
- `POST /api/v1/customers` - Create a new customer
- `PUT /api/v1/customers/{id}` - Update customer
- `DELETE /api/v1/customers/{id}` - Delete customer
- `GET /api/v1/customers/{id}/orders` - Get customer's orders

#### Orders

- `GET /api/v1/orders` - List all orders
- `GET /api/v1/orders/{id}` - Get order by ID
- `POST /api/v1/orders` - Create a new order
- `PATCH /api/v1/orders/{id}` - Update order (partial)
- `POST /api/v1/orders/{id}/pets` - Add pet to order
- `DELETE /api/v1/orders/{id}/pets/{petId}` - Remove pet from order
- `POST /api/v1/orders/{id}/transition` - Transition order status

### Order Status Lifecycle

Orders progress through three states:

1. **Open**: Order can be modified (add/remove pets, update pickup date)
2. **Processing**: Only pickup date can be modified
3. **Delivered**: Order is immutable (read-only)

### Cost Calculation

- **Estimated Cost**: Calculated from current pet prices (for Open/Processing orders)
- **Actual Cost**: Persisted value set when order transitions to Delivered

For detailed API contracts and examples, see:
- [dx/docs/api-reference.md](dx/docs/api-reference.md) - Complete API reference
- [dx/docs/order-lifecycle.md](dx/docs/order-lifecycle.md) - Order state management
- [dx/docs/getting-started.md](dx/docs/getting-started.md) - Quick start guide

## 🧪 Testing

The solution includes comprehensive test coverage:

- **Domain Tests**: Business rule validation and entity behavior
- **Application Tests**: Service layer orchestration and business logic
- **API Tests**: End-to-end HTTP endpoint testing
- **Contract Tests**: API versioning and contract validation

### Run Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test project
dotnet test --project PetShop.Tests
```

For testing strategies and best practices, see the test projects in `PetShop.Tests/`.

## 🛠️ Tech Stack

### Core Framework
- **.NET 8.0**: Latest LTS release
- **ASP.NET Core**: Web framework for REST API
- **Entity Framework Core**: ORM for data access

### Persistence
- **EF Core InMemory Provider**: Development and testing database
- Designed for easy migration to SQL Server, PostgreSQL, or Cosmos DB

### Testing
- **xUnit**: Test framework
- **FluentAssertions**: Readable assertion library

### Documentation
- **Swagger/OpenAPI**: Interactive API documentation
- **XML Documentation Comments**: Inline code documentation

### DevOps
- **Docker**: Containerization
- **GitHub Actions**: CI/CD pipelines

## 📖 Documentation

### Public Documentation

Comprehensive, public-facing documentation is available in the `dx/docs/` directory:

- **[Getting Started](dx/docs/getting-started.md)**: Quick start guide with step-by-step examples
- **[API Reference](dx/docs/api-reference.md)**: Complete endpoint documentation with request/response examples
- **[Architecture](dx/docs/architecture.md)**: System architecture overview and design principles
- **[Order Lifecycle](dx/docs/order-lifecycle.md)**: Detailed order state management and transitions

Start with the [DX Documentation Index](dx/README.md) for an overview of all available documentation.

### Deployment Documentation

- **[README-DEPLOYMENT.md](README-DEPLOYMENT.md)**: Quick deployment guide with Docker Compose
- **[docs/DEPLOYMENT.md](docs/DEPLOYMENT.md)**: Detailed production deployment guide (internal development docs)

## 🔒 Important Notes

### In-Memory Database

⚠️ **Current Setup Uses InMemory Database**

- Data is **lost when the application restarts**
- **NOT suitable for production**
- Configure a real database (SQL Server, PostgreSQL) before production use

See [README-DEPLOYMENT.md](README-DEPLOYMENT.md) and [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for production database configuration.

### Authentication & Authorization

Authentication and authorization are **not implemented** in this version. They are assumed to be handled by:
- API Gateway
- OAuth 2.0 / JWT middleware
- API keys

## 🎯 Project Status

This project represents a complete implementation of the original requirements with additional production-ready features:

- ✅ All core requirements implemented
- ✅ Clean Architecture structure
- ✅ Comprehensive test coverage
- ✅ API versioning
- ✅ Docker deployment support
- ✅ CI/CD pipeline setup
- ✅ Extensive documentation

### Future Enhancements

Potential areas for future development:
- Production database integration (SQL Server/PostgreSQL)
- Authentication and authorization
- Pagination and filtering
- Caching layer
- Event sourcing for audit trails
- CQRS pattern for read/write separation

## 🤝 Contributing

This project was built as part of a technical assessment. For questions or contributions, please refer to the original requirements in [ORIGINAL-REQUIREMENTS.md](ORIGINAL-REQUIREMENTS.md).

## 📋 Project Phases

This project was implemented in phases following Clean Architecture principles:

- ✅ **Phase 1**: Solution Setup and Foundation
- ✅ **Phase 2**: Domain Modeling
- ✅ **Phase 3**: Application Services
- ✅ **Phase 4**: Infrastructure
- ✅ **Phase 5**: API Layer
- ✅ **Phase 6**: Testing
- ✅ **Phase 7**: Hardening and Polish
- 🔄 **Phase 8**: Developer Experience Portal (future enhancement)

All phases through Phase 7 are complete and verified. See `execution/phases/` for detailed phase documentation.

## 📝 License

This project is part of a technical assessment. See the repository for license details.

---

**Built with ❤️ using .NET 8.0 and Clean Architecture principles**
