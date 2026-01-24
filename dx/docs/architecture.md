# Architecture Overview

The Pet Shop API is built using **Clean Architecture** principles, ensuring separation of concerns, testability, and maintainability. This document provides a high-level overview of the system's architecture and design decisions.

## Architecture Layers

The solution is organized into four main layers, each with distinct responsibilities:

```
┌─────────────────────────────────────┐
│         PetShop.Api                 │  ← Presentation Layer
│  (HTTP endpoints, DTOs, Swagger)    │
└──────────────┬──────────────────────┘
               │ depends on
               ▼
┌─────────────────────────────────────┐
│      PetShop.Application            │  ← Application Layer
│  (Use cases, orchestration, DTOs)   │
└──────────────┬──────────────────────┘
               │ depends on
               ▼
┌─────────────────────────────────────┐
│        PetShop.Domain                │  ← Domain Layer
│  (Business logic, entities, rules)  │
└─────────────────────────────────────┘
               ▲
               │ depends on
┌──────────────┴──────────────────────┐
│     PetShop.Infrastructure           │  ← Infrastructure Layer
│  (DbContext, repositories, data)     │
└──────────────────────────────────────┘
```

## Layer Responsibilities

### API Layer (PetShop.Api)

**Purpose**: Handle HTTP requests and responses, API-specific concerns.

**Responsibilities**:
- Define REST endpoints
- Map HTTP requests to application service calls
- Convert domain models to DTOs
- Handle HTTP status codes and error responses
- Configure Swagger/OpenAPI documentation
- Input validation at API boundaries

**Key Principle**: The API layer is a thin translation layer. It contains **no business logic**.

### Application Layer (PetShop.Application)

**Purpose**: Orchestrate domain operations to fulfill use cases.

**Responsibilities**:
- Implement application services (e.g., `CustomerService`, `OrderService`)
- Coordinate multiple domain entities
- Handle transaction boundaries
- Validate business rules that span entities
- Convert between domain models and DTOs

**Key Principle**: Application services coordinate domain objects but delegate business rules to the domain layer.

### Domain Layer (PetShop.Domain)

**Purpose**: Core business logic, entities, and rules.

**Responsibilities**:
- Define business entities (Customer, Order, Pet)
- Implement business rules and invariants
- Define value objects (OrderStatus)
- Throw domain exceptions for rule violations
- Enforce entity state transitions

**Key Principle**: The domain layer has **zero dependencies** on infrastructure or frameworks. It's pure business logic.

### Infrastructure Layer (PetShop.Infrastructure)

**Purpose**: Implement persistence and external integrations.

**Responsibilities**:
- Configure Entity Framework Core DbContext
- Implement repository patterns (if used)
- Handle database migrations
- Integrate with external services (future)

**Key Principle**: Infrastructure implements abstractions defined in the domain/application layers.

## Design Principles

### 1. Dependency Inversion

Dependencies point **inward** toward the domain:

- API depends on Application
- Application depends on Domain
- Infrastructure depends on Domain
- **Domain depends on nothing**

This ensures business logic is independent of infrastructure choices.

### 2. Separation of Concerns

Each layer has a single, well-defined responsibility:

- **API**: HTTP concerns
- **Application**: Use case orchestration
- **Domain**: Business rules
- **Infrastructure**: Data persistence

### 3. Testability

The architecture enables comprehensive testing:

- **Domain**: Pure unit tests (no mocks needed)
- **Application**: Test with mocked infrastructure
- **API**: Integration tests with in-memory test server

### 4. Maintainability

Clear boundaries make the system easy to maintain:

- Changes in one layer don't cascade unnecessarily
- New features follow a predictable pattern
- Onboarding is straightforward (read layer-by-layer)

## Key Design Patterns

### Repository Pattern

Repositories abstract data access:

- Interfaces defined in Application layer
- Implementations in Infrastructure layer
- Enables easy testing with in-memory implementations

### Service Layer Pattern

Application services encapsulate use cases:

- Each service method = one business operation
- Examples: `CreateOrder`, `AddPetToOrder`, `TransitionOrderToProcessing`
- Services coordinate domain entities

### DTO Pattern

Data Transfer Objects decouple layers:

- API uses DTOs for request/response
- Application uses DTOs for inter-layer communication
- Domain models never exposed directly

## Data Persistence

### Current: In-Memory Database

The API uses EF Core InMemory provider for development:

- **Pros**: Fast startup, no database setup, isolated tests
- **Cons**: Data lost on restart, not suitable for production

### Production: Relational Database

The architecture supports easy migration to:

- SQL Server
- PostgreSQL
- Other relational databases

**Migration Path**: Update Infrastructure layer configuration only. No changes to Domain, Application, or API layers.

## Error Handling Strategy

Errors flow from domain to API:

1. **Domain**: Throws domain exceptions (`InvalidOrderStateException`, `BusinessRuleViolationException`)
2. **Application**: Catches and enriches exceptions
3. **API**: Maps exceptions to HTTP status codes and error responses

This ensures consistent error handling across all layers.

## Testing Strategy

Tests mirror the architecture:

- **Domain Tests**: Unit tests for business rules
- **Application Tests**: Integration tests with mocked infrastructure
- **API Tests**: End-to-end tests with in-memory test server

Each layer can be tested independently, ensuring comprehensive coverage.

## Scalability Considerations

The architecture supports future growth:

### Horizontal Scaling

- Stateless API layer can scale horizontally
- Application services are stateless
- Database can be scaled independently

### Feature Growth

New features follow a predictable pattern:

1. Add domain entities/value objects
2. Add application services
3. Add API endpoints
4. Update infrastructure if needed

### Microservices Migration

The current architecture can be extracted into microservices:

- Each service contains its own API, Application, Domain, Infrastructure layers
- Services communicate via HTTP or messaging
- Clear boundaries make extraction straightforward

## Technology Stack

- **.NET 8.0**: Latest LTS release
- **ASP.NET Core**: Web framework
- **Entity Framework Core**: ORM for data access
- **xUnit**: Testing framework
- **Swagger/OpenAPI**: API documentation

## Benefits of This Architecture

1. **Testability**: Business logic can be tested without infrastructure
2. **Flexibility**: Easy to change persistence or add features
3. **Maintainability**: Clear boundaries and responsibilities
4. **Onboarding**: New developers can understand the system quickly
5. **Future-Proof**: Architecture supports growth and change

## Further Reading

- [API Reference](api-reference.md) - Complete API documentation
- [Order Lifecycle](order-lifecycle.md) - Order state management
- [Getting Started](getting-started.md) - Setup and first steps

---

This architecture ensures the Pet Shop API is maintainable, testable, and ready for production use.
