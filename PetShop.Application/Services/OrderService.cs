using PetShop.Application.DTOs;
using PetShop.Application.Repositories;
using PetShop.Domain;

namespace PetShop.Application.Services;

/// <summary>
/// Application service for order-related operations.
/// Orchestrates order CRUD operations, state transitions, pet management, and cost calculations.
/// </summary>
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderService"/> class.
    /// </summary>
    /// <param name="orderRepository">The order repository for data access.</param>
    /// <param name="customerRepository">The customer repository for validation.</param>
    public OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="request">The order creation request.</param>
    /// <returns>The created order as a DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when customer is not found.</exception>
    /// <exception cref="BusinessRuleViolationException">Thrown when validation fails (handled by domain entity).</exception>
    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {request.CustomerId} was not found.");
        }

        var order = new Order(request.CustomerId, request.PickupDate);
        var createdOrder = await _orderRepository.CreateAsync(order);

        return MapToDto(createdOrder);
    }

    /// <summary>
    /// Gets an order by its unique identifier with calculated cost.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The order as a DTO with cost calculation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when order is not found.</exception>
    public async Task<OrderDto> GetOrderByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} was not found.");
        }

        return MapToDto(order);
    }

    /// <summary>
    /// Updates an existing order's pickup date.
    /// </summary>
    /// <param name="id">The unique identifier of the order to update.</param>
    /// <param name="request">The order update request.</param>
    /// <returns>The updated order as a DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when order is not found.</exception>
    /// <exception cref="InvalidOrderStateException">Thrown when order is in Delivered status.</exception>
    /// <exception cref="BusinessRuleViolationException">Thrown when validation fails (handled by domain entity).</exception>
    public async Task<OrderDto> UpdateOrderAsync(Guid id, UpdateOrderRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} was not found.");
        }

        order.UpdatePickupDate(request.PickupDate);
        var updatedOrder = await _orderRepository.UpdateAsync(order);

        return MapToDto(updatedOrder);
    }

    /// <summary>
    /// Transitions an order from Open to Processing status.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The updated order as a DTO.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when order is not found.</exception>
    /// <exception cref="InvalidOrderStateException">Thrown when order is not in Open status.</exception>
    /// <exception cref="BusinessRuleViolationException">Thrown when order has no pets.</exception>
    public async Task<OrderDto> TransitionOrderToProcessingAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} was not found.");
        }

        order.TransitionToProcessing();
        var updatedOrder = await _orderRepository.UpdateAsync(order);

        return MapToDto(updatedOrder);
    }

    /// <summary>
    /// Transitions an order from Processing to Delivered status.
    /// Sets the ActualCost to the sum of all pet prices at delivery time.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The updated order as a DTO.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when order is not found.</exception>
    /// <exception cref="InvalidOrderStateException">Thrown when order is not in Processing status.</exception>
    public async Task<OrderDto> TransitionOrderToDeliveredAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} was not found.");
        }

        order.TransitionToDelivered();
        var updatedOrder = await _orderRepository.UpdateAsync(order);

        return MapToDto(updatedOrder);
    }

    /// <summary>
    /// Adds a pet to an order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="request">The pet creation request.</param>
    /// <returns>The updated order as a DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when order is not found.</exception>
    /// <exception cref="InvalidOrderStateException">Thrown when order is not in Open status.</exception>
    /// <exception cref="BusinessRuleViolationException">Thrown when validation fails (handled by domain entity).</exception>
    public async Task<OrderDto> AddPetToOrderAsync(Guid orderId, AddPetRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} was not found.");
        }

        var pet = new Pet(orderId, request.Name, request.Price, request.Kind, request.Color);
        order.AddPet(pet);
        var updatedOrder = await _orderRepository.UpdateAsync(order);

        return MapToDto(updatedOrder);
    }

    /// <summary>
    /// Removes a pet from an order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="petId">The unique identifier of the pet to remove.</param>
    /// <returns>The updated order as a DTO.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when order or pet is not found.</exception>
    /// <exception cref="InvalidOrderStateException">Thrown when order is not in Open status.</exception>
    /// <exception cref="ArgumentException">Thrown when pet is not in the order.</exception>
    public async Task<OrderDto> RemovePetFromOrderAsync(Guid orderId, Guid petId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {orderId} was not found.");
        }

        var pet = order.Pets.FirstOrDefault(p => p.Id == petId);
        if (pet == null)
        {
            throw new KeyNotFoundException($"Pet with ID {petId} was not found in order {orderId}.");
        }

        order.RemovePet(pet);
        var updatedOrder = await _orderRepository.UpdateAsync(order);

        return MapToDto(updatedOrder);
    }

    /// <summary>
    /// Maps a domain Order entity to an OrderDto.
    /// Calculates cost based on order status.
    /// </summary>
    /// <param name="order">The order entity to map.</param>
    /// <returns>The mapped OrderDto.</returns>
    private static OrderDto MapToDto(Order order)
    {
        var dto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            PickupDate = order.PickupDate,
            Pets = order.Pets.Select(p => new PetDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Kind = p.Kind,
                Color = p.Color
            }).ToList()
        };

        // Calculate cost based on status
        if (order.Status == OrderStatus.Delivered)
        {
            dto.ActualCost = order.ActualCost;
            dto.EstimatedCost = null;
        }
        else
        {
            dto.EstimatedCost = order.CalculateEstimatedCost();
            dto.ActualCost = null;
        }

        return dto;
    }
}
