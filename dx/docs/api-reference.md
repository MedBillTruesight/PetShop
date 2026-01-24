# API Reference

Complete reference for all Pet Shop API endpoints, request/response formats, and error codes.

## Base URL

```
http://localhost:7052/api/v1
```

## API Versioning

The API uses URL-based versioning. All endpoints are prefixed with `/api/v1/`. Future versions will use `/api/v2/`, etc.

## Authentication

Authentication is not implemented in the current version. It is assumed to be handled by:
- API Gateway
- OAuth 2.0 / JWT middleware
- API keys

## Content Type

All requests and responses use `application/json`.

## HTTP Status Codes

### Success Codes

- `200 OK` - Successful GET, PUT, PATCH, DELETE
- `201 Created` - Successful POST with resource creation
- `204 No Content` - Successful DELETE with no response body

### Client Error Codes

- `400 Bad Request` - Invalid request format or validation failure
- `404 Not Found` - Resource does not exist
- `409 Conflict` - Business rule violation (e.g., invalid state transition)
- `422 Unprocessable Entity` - Semantic validation failure (e.g., pickup date in past)

### Server Error Codes

- `500 Internal Server Error` - Unexpected server error

## Error Response Format

All errors follow this structure:

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error message",
    "details": {
      "field": "additional context"
    }
  }
}
```

## Customer Endpoints

### List All Customers

```http
GET /api/v1/customers
```

**Response:** `200 OK`

```json
[
  {
    "id": "guid",
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "phone": "555-0123",
    "estimatedPaymentDue": 299.99,
    "actualPaymentDue": 150.00
  }
]
```

### Get Customer by ID

```http
GET /api/v1/customers/{id}
```

**Parameters:**
- `id` (path, required) - Customer GUID

**Response:** `200 OK`

```json
{
  "id": "guid",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "555-0123",
  "estimatedPaymentDue": 299.99,
  "actualPaymentDue": 150.00
}
```

**Payment Fields:**
- `estimatedPaymentDue`: Sum of estimated costs from orders with status `Open` or `Processing`
- `actualPaymentDue`: Sum of actual costs from orders with status `Delivered`

### Create Customer

```http
POST /api/v1/customers
```

**Request Body:**

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "555-0123"
}
```

**Validation:**
- `firstName` (required): Non-empty string
- `lastName` (required): Non-empty string
- `email` (optional): Valid email format
- `phone` (optional): String

**Response:** `201 Created`

```json
{
  "id": "guid",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phone": "555-0123",
  "estimatedPaymentDue": 0.00,
  "actualPaymentDue": 0.00
}
```

### Update Customer

```http
PUT /api/v1/customers/{id}
```

**Parameters:**
- `id` (path, required) - Customer GUID

**Request Body:**

```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane.doe@example.com",
  "phone": "555-0124"
}
```

**Response:** `200 OK` (same structure as Get Customer)

### Delete Customer

```http
DELETE /api/v1/customers/{id}
```

**Parameters:**
- `id` (path, required) - Customer GUID

**Response:** `204 No Content`

### Get Customer Orders

```http
GET /api/v1/customers/{id}/orders
```

**Parameters:**
- `id` (path, required) - Customer GUID

**Response:** `200 OK`

```json
[
  {
    "id": "guid",
    "customerId": "guid",
    "status": "Open",
    "pickupDate": "2026-02-01T00:00:00",
    "pets": [],
    "estimatedCost": 299.99,
    "actualCost": null
  }
]
```

## Order Endpoints

### List All Orders

```http
GET /api/v1/orders
```

**Response:** `200 OK`

```json
[
  {
    "id": "guid",
    "customerId": "guid",
    "status": "Open",
    "pickupDate": "2026-02-01T00:00:00",
    "pets": [],
    "estimatedCost": 299.99,
    "actualCost": null
  }
]
```

### Get Order by ID

```http
GET /api/v1/orders/{id}
```

**Parameters:**
- `id` (path, required) - Order GUID

**Response:** `200 OK`

```json
{
  "id": "guid",
  "customerId": "guid",
  "status": "Open",
  "pickupDate": "2026-02-01T00:00:00",
  "pets": [
    {
      "id": "guid",
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

**Cost Fields:**
- `estimatedCost`: Sum of pet prices (when status is `Open` or `Processing`)
- `actualCost`: Persisted value (when status is `Delivered`), `null` otherwise

### Create Order

```http
POST /api/v1/orders
```

**Request Body:**

```json
{
  "customerId": "guid",
  "pickupDate": "2026-02-01"
}
```

**Validation:**
- `customerId` (required): Valid customer GUID
- `pickupDate` (required): Date in format `YYYY-MM-DD`, must be today or in the future

**Response:** `201 Created`

```json
{
  "id": "guid",
  "customerId": "guid",
  "status": "Open",
  "pickupDate": "2026-02-01T00:00:00",
  "pets": [],
  "estimatedCost": 0.00,
  "actualCost": null
}
```

**Note:** Orders are created with status `Open` by default.

### Update Order

```http
PATCH /api/v1/orders/{id}
```

**Parameters:**
- `id` (path, required) - Order GUID

**Request Body:**

```json
{
  "pickupDate": "2026-02-05"
}
```

**Validation:**
- Only fields provided are updated (partial update)
- `pickupDate` must be today or in the future
- Modification restrictions based on order status (see [Order Lifecycle](order-lifecycle.md))

**Response:** `200 OK` (same structure as Get Order)

**Status Restrictions:**
- `Open`: Can update pickup date, add/remove pets
- `Processing`: Can only update pickup date
- `Delivered`: No modifications allowed (returns `409 Conflict`)

### Transition Order Status

```http
POST /api/v1/orders/{id}/transition
```

**Parameters:**
- `id` (path, required) - Order GUID

**Request Body:**

```json
{
  "targetStatus": "Processing"
}
```

**Valid Status Values:**
- `Processing` - Transition from `Open` to `Processing`
- `Delivered` - Transition from `Processing` to `Delivered`

**Validation:**
- `Open` → `Processing`: Order must have at least one pet
- `Processing` → `Delivered`: No restrictions
- `Delivered` → Any: Forbidden (order is immutable)

**Response:** `200 OK` (same structure as Get Order)

**Error:** `409 Conflict` if transition is invalid

```json
{
  "error": {
    "code": "INVALID_STATE_TRANSITION",
    "message": "Cannot transition from Open to Delivered. Order must be Processing first.",
    "details": {
      "orderId": "guid",
      "currentStatus": "Open",
      "requestedStatus": "Delivered"
    }
  }
}
```

## Pet Endpoints

Pets exist only within orders. There are no top-level pet resources.

### Add Pet to Order

```http
POST /api/v1/orders/{id}/pets
```

**Parameters:**
- `id` (path, required) - Order GUID

**Request Body:**

```json
{
  "name": "Fluffy",
  "kind": "Cat",
  "color": "White",
  "price": 299.99
}
```

**Validation:**
- `name` (required): Non-empty string
- `price` (required): Positive decimal number
- `kind` (optional): String
- `color` (optional): String
- Order status must be `Open` (returns `409 Conflict` if not)

**Response:** `201 Created`

```json
{
  "id": "guid",
  "name": "Fluffy",
  "kind": "Cat",
  "color": "White",
  "price": 299.99
}
```

### Remove Pet from Order

```http
DELETE /api/v1/orders/{id}/pets/{petId}
```

**Parameters:**
- `id` (path, required) - Order GUID
- `petId` (path, required) - Pet GUID

**Validation:**
- Order status must be `Open` (returns `409 Conflict` if not)
- Pet must exist in the order (returns `404 Not Found` if not)

**Response:** `204 No Content`

## Error Codes

### Validation Errors (`400 Bad Request`)

- `VALIDATION_ERROR` - General validation failure
- `MISSING_REQUIRED_FIELD` - Required field not provided
- `INVALID_FORMAT` - Field format is invalid (e.g., email, date)

### Business Rule Violations (`409 Conflict`)

- `ORDER_INVALID_STATE` - Order state transition or modification violates state rules
- `PET_LIST_IMMUTABLE` - Attempt to modify pets when not allowed
- `ORDER_IMMUTABLE` - Attempt to modify delivered order
- `INVALID_STATE_TRANSITION` - Invalid order status transition

### Not Found (`404 Not Found`)

- `CUSTOMER_NOT_FOUND` - Customer ID does not exist
- `ORDER_NOT_FOUND` - Order ID does not exist
- `PET_NOT_FOUND` - Pet ID does not exist or does not belong to order

### Semantic Validation (`422 Unprocessable Entity`)

- `INVALID_PICKUP_DATE` - Pickup date is in the past
- `INVALID_DATE_RANGE` - Date range validation failure

## Examples

### Complete Order Flow

1. **Create Customer**
   ```bash
   POST /api/v1/customers
   {"firstName":"John","lastName":"Doe"}
   ```

2. **Create Order**
   ```bash
   POST /api/v1/orders
   {"customerId":"<customer-id>","pickupDate":"2026-02-01"}
   ```

3. **Add Pet**
   ```bash
   POST /api/v1/orders/<order-id>/pets
   {"name":"Fluffy","kind":"Cat","price":299.99}
   ```

4. **Transition to Processing**
   ```bash
   POST /api/v1/orders/<order-id>/transition
   {"targetStatus":"Processing"}
   ```

5. **Transition to Delivered**
   ```bash
   POST /api/v1/orders/<order-id>/transition
   {"targetStatus":"Delivered"}
   ```

## Interactive Documentation

For interactive API exploration, use Swagger UI:

```
http://localhost:7052/swagger
```

Swagger UI provides:
- Complete endpoint documentation
- Try-it-out functionality
- Request/response examples
- Schema definitions

## Further Reading

- [Getting Started](getting-started.md) - Setup and first steps
- [Order Lifecycle](order-lifecycle.md) - Detailed order state management
- [Architecture](architecture.md) - System architecture overview

---

This reference covers all current API endpoints. For questions or clarifications, please refer to the Swagger UI or open an issue in the repository.
