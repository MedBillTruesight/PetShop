# Order Lifecycle

Orders in the Pet Shop API progress through three distinct states: `Open`, `Processing`, and `Delivered`. Understanding these states and their transitions is essential for integrating with the API.

## Order States

### Open

**Description**: The initial state when an order is created. Orders in this state can be freely modified.

**Characteristics:**
- Order exists but has not been processed
- Pets can be added or removed
- Pickup date can be modified
- Customer cannot be changed (set at creation)
- Status can be changed to `Processing` (with validation)

**Cost Calculation**: `estimatedCost` = sum of all pet prices

**Allowed Operations:**
- ✅ Add pets (`POST /api/v1/orders/{id}/pets`)
- ✅ Remove pets (`DELETE /api/v1/orders/{id}/pets/{petId}`)
- ✅ Update pickup date (`PATCH /api/v1/orders/{id}`)
- ✅ Transition to `Processing` (`POST /api/v1/orders/{id}/transition`)

**Business Rules:**
- Order must have at least one pet before transitioning to `Processing`
- Pickup date must be today or in the future

### Processing

**Description**: Order has been confirmed and is being prepared for pickup. Limited modifications allowed.

**Characteristics:**
- Order is locked for most changes
- Pets cannot be added or removed
- Customer cannot be changed
- Only pickup date can be modified
- Status can be changed to `Delivered`

**Cost Calculation**: `estimatedCost` = sum of all pet prices (still estimated, not persisted)

**Allowed Operations:**
- ✅ Update pickup date (`PATCH /api/v1/orders/{id}`)
- ✅ Transition to `Delivered` (`POST /api/v1/orders/{id}/transition`)

**Forbidden Operations:**
- ❌ Adding pets
- ❌ Removing pets
- ❌ Changing customer
- ❌ Changing any field except pickup date
- ❌ Reverting to `Open`

**Rationale**: Once in Processing, the order represents a commitment. Allowing pet changes would create confusion about what the customer actually receives.

### Delivered

**Description**: Order has been completed. The order is immutable and represents a historical record.

**Characteristics:**
- Order cannot be modified in any way
- All fields are read-only
- Actual cost is persisted at transition time
- Status cannot be changed

**Cost Calculation**: `actualCost` = persisted value (set when status changed to `Delivered`)

**Allowed Operations:**
- ✅ Read order (`GET /api/v1/orders/{id}`)

**Forbidden Operations:**
- ❌ All modifications (order is immutable)
- ❌ All state transitions

**Rationale**: Delivered orders are historical records. Modifying them would compromise audit integrity and customer trust.

## State Transitions

### Valid Transitions

```
Open → Processing
  Condition: Order must have at least one pet
  Action: Lock pet list, allow only pickup date changes

Processing → Delivered
  Condition: None (unconditional)
  Action: Persist actual cost, make order immutable
```

### Invalid Transitions

All other transitions are forbidden:

- `Open` → `Delivered` (must go through `Processing`)
- `Processing` → `Open` (cannot revert)
- `Delivered` → Any state (immutable)

**Rationale**: The three-state model ensures orders progress linearly. Skipping `Processing` or reverting states would complicate business logic and audit trails.

## Transition Examples

### Transition from Open to Processing

**Request:**
```http
POST /api/v1/orders/{id}/transition
Content-Type: application/json

{
  "targetStatus": "Processing"
}
```

**Success Response:** `200 OK`

**Error Response:** `409 Conflict` (if order has no pets)

```json
{
  "error": {
    "code": "INVALID_STATE_TRANSITION",
    "message": "Order must have at least one pet to transition to Processing",
    "details": {
      "orderId": "guid",
      "currentStatus": "Open",
      "requestedStatus": "Processing"
    }
  }
}
```

### Transition from Processing to Delivered

**Request:**
```http
POST /api/v1/orders/{id}/transition
Content-Type: application/json

{
  "targetStatus": "Delivered"
}
```

**Success Response:** `200 OK`

When an order transitions to `Delivered`, the `actualCost` is automatically calculated and persisted.

## Cost Calculation

### Estimated Cost (Open/Processing)

When an order is `Open` or `Processing`, the API calculates cost dynamically:

```
estimatedCost = sum of all pet prices
```

**Characteristics:**
- Calculated on every read
- Reflects current pet prices
- Not persisted to database
- Can change if pet prices are modified (though pets should be immutable in Processing)

**Use Case**: Customers see expected payment before delivery.

**Example:**
```json
{
  "id": "guid",
  "status": "Open",
  "pets": [
    {"name": "Fluffy", "price": 299.99},
    {"name": "Max", "price": 199.99}
  ],
  "estimatedCost": 499.98,
  "actualCost": null
}
```

### Actual Cost (Delivered)

When an order transitions to `Delivered`, the actual cost is calculated and persisted:

```
actualCost = sum of all pet prices (at delivery time)
```

**Characteristics:**
- Calculated once at delivery time
- Persisted to database
- Never changes after persistence
- Returned on all future reads

**Use Case**: Historical record of what customer actually paid.

**Example:**
```json
{
  "id": "guid",
  "status": "Delivered",
  "pets": [
    {"name": "Fluffy", "price": 299.99},
    {"name": "Max", "price": 199.99}
  ],
  "estimatedCost": null,
  "actualCost": 499.98
}
```

### Why Persist Actual Cost?

1. **Price Changes**: If pet prices change after order creation, estimated cost would change, but actual cost remains accurate.
2. **Audit Trail**: Persisted cost provides immutable record for accounting and disputes.
3. **Performance**: No need to recalculate on every read for delivered orders.

## Error Scenarios

### Attempting Invalid State Transition

**Request:**
```http
POST /api/v1/orders/{id}/transition
{"targetStatus": "Delivered"}
```

When order status is `Open`:

**Response:** `409 Conflict`

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

### Attempting to Modify Delivered Order

**Request:**
```http
PATCH /api/v1/orders/{id}
{"pickupDate": "2026-02-10"}
```

When order status is `Delivered`:

**Response:** `409 Conflict`

```json
{
  "error": {
    "code": "ORDER_IMMUTABLE",
    "message": "Order cannot be modified when status is Delivered",
    "details": {
      "orderId": "guid",
      "status": "Delivered"
    }
  }
}
```

### Attempting to Add Pet to Processing Order

**Request:**
```http
POST /api/v1/orders/{id}/pets
{"name": "Buddy", "price": 150.00}
```

When order status is `Processing`:

**Response:** `409 Conflict`

```json
{
  "error": {
    "code": "PET_LIST_IMMUTABLE",
    "message": "Cannot add pets to order in Processing state",
    "details": {
      "orderId": "guid",
      "status": "Processing"
    }
  }
}
```

## Complete Order Flow Example

Here's a complete example of an order progressing through all states:

### 1. Create Order (Status: Open)

```http
POST /api/v1/orders
{
  "customerId": "customer-guid",
  "pickupDate": "2026-02-01"
}
```

**Response:**
```json
{
  "id": "order-guid",
  "status": "Open",
  "pets": [],
  "estimatedCost": 0.00,
  "actualCost": null
}
```

### 2. Add Pets (Status: Open)

```http
POST /api/v1/orders/order-guid/pets
{"name": "Fluffy", "kind": "Cat", "price": 299.99}

POST /api/v1/orders/order-guid/pets
{"name": "Max", "kind": "Dog", "price": 199.99}
```

**Response (after both pets added):**
```json
{
  "id": "order-guid",
  "status": "Open",
  "pets": [
    {"name": "Fluffy", "price": 299.99},
    {"name": "Max", "price": 199.99}
  ],
  "estimatedCost": 499.98,
  "actualCost": null
}
```

### 3. Transition to Processing

```http
POST /api/v1/orders/order-guid/transition
{"targetStatus": "Processing"}
```

**Response:**
```json
{
  "id": "order-guid",
  "status": "Processing",
  "pets": [
    {"name": "Fluffy", "price": 299.99},
    {"name": "Max", "price": 199.99}
  ],
  "estimatedCost": 499.98,
  "actualCost": null
}
```

**Note**: Pet list is now locked. Only pickup date can be modified.

### 4. Transition to Delivered

```http
POST /api/v1/orders/order-guid/transition
{"targetStatus": "Delivered"}
```

**Response:**
```json
{
  "id": "order-guid",
  "status": "Delivered",
  "pets": [
    {"name": "Fluffy", "price": 299.99},
    {"name": "Max", "price": 199.99}
  ],
  "estimatedCost": null,
  "actualCost": 499.98
}
```

**Note**: Order is now immutable. `actualCost` is persisted.

## State Diagram

```
┌─────────┐
│  Open   │
│         │
│ • Add   │
│   pets  │
│ • Remove│
│   pets  │
│ • Update│
│   date  │
└────┬────┘
     │ (has pets)
     ▼
┌─────────────┐
│ Processing  │
│             │
│ • Update    │
│   date only │
└────┬────────┘
     │ (unconditional)
     ▼
┌─────────────┐
│  Delivered  │
│             │
│ • Read-only │
│ • Immutable │
└─────────────┘
```

## Best Practices

1. **Always check order status** before attempting modifications
2. **Validate pet count** before transitioning to Processing
3. **Handle 409 Conflict errors** gracefully in your client
4. **Use actualCost** for delivered orders, not estimatedCost
5. **Don't attempt to modify** delivered orders

## Further Reading

- [API Reference](api-reference.md) - Complete endpoint documentation
- [Getting Started](getting-started.md) - Setup and examples
- [Architecture](architecture.md) - System design overview

---

Understanding the order lifecycle is crucial for building reliable integrations with the Pet Shop API.
