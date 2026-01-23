using PetShop.Application.DTOs;
using PetShop.Application.Repositories;
using PetShop.Domain;

namespace PetShop.Application.Services;

/// <summary>
/// Application service for customer-related operations.
/// Orchestrates customer CRUD operations and calculates payment due amounts.
/// </summary>
public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerService"/> class.
    /// </summary>
    /// <param name="customerRepository">The customer repository for data access.</param>
    /// <param name="orderRepository">The order repository for payment calculations.</param>
    public CustomerService(ICustomerRepository customerRepository, IOrderRepository orderRepository)
    {
        _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="request">The customer creation request.</param>
    /// <returns>The created customer as a DTO.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="BusinessRuleViolationException">Thrown when validation fails (handled by domain entity).</exception>
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var customer = new Customer(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone);

        var createdCustomer = await _customerRepository.CreateAsync(customer);

        return MapToDto(createdCustomer, estimatedPaymentDue: 0m, actualPaymentDue: 0m);
    }

    /// <summary>
    /// Gets a customer by their unique identifier with calculated payment due amounts.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>The customer as a DTO with payment calculations.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when customer is not found.</exception>
    public async Task<CustomerDto> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} was not found.");
        }

        var orders = await _orderRepository.GetByCustomerIdAsync(id);

        var estimatedPaymentDue = CalculateEstimatedPaymentDue(orders);
        var actualPaymentDue = CalculateActualPaymentDue(orders);

        return MapToDto(customer, estimatedPaymentDue, actualPaymentDue);
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="request">The customer update request.</param>
    /// <returns>The updated customer as a DTO with payment calculations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when customer is not found.</exception>
    /// <exception cref="BusinessRuleViolationException">Thrown when validation fails (handled by domain entity).</exception>
    public async Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            throw new KeyNotFoundException($"Customer with ID {id} was not found.");
        }

        customer.UpdateFirstName(request.FirstName);
        customer.UpdateLastName(request.LastName);
        customer.UpdateEmail(request.Email);
        customer.UpdatePhone(request.Phone);

        var updatedCustomer = await _customerRepository.UpdateAsync(customer);

        var orders = await _orderRepository.GetByCustomerIdAsync(id);

        var estimatedPaymentDue = CalculateEstimatedPaymentDue(orders);
        var actualPaymentDue = CalculateActualPaymentDue(orders);

        return MapToDto(updatedCustomer, estimatedPaymentDue, actualPaymentDue);
    }

    /// <summary>
    /// Calculates the estimated payment due from non-delivered orders.
    /// Sum of estimated costs from orders with status Open or Processing.
    /// </summary>
    /// <param name="orders">The collection of orders for the customer.</param>
    /// <returns>The total estimated payment due.</returns>
    private static decimal CalculateEstimatedPaymentDue(IEnumerable<Order> orders)
    {
        return orders
            .Where(o => o.Status == OrderStatus.Open || o.Status == OrderStatus.Processing)
            .Sum(o => o.CalculateEstimatedCost());
    }

    /// <summary>
    /// Calculates the actual payment due from delivered orders.
    /// Sum of actual costs from orders with status Delivered.
    /// </summary>
    /// <param name="orders">The collection of orders for the customer.</param>
    /// <returns>The total actual payment due.</returns>
    private static decimal CalculateActualPaymentDue(IEnumerable<Order> orders)
    {
        return orders
            .Where(o => o.Status == OrderStatus.Delivered && o.ActualCost.HasValue)
            .Sum(o => o.ActualCost!.Value);
    }

    /// <summary>
    /// Maps a domain Customer entity to a CustomerDto.
    /// </summary>
    /// <param name="customer">The customer entity to map.</param>
    /// <param name="estimatedPaymentDue">The calculated estimated payment due.</param>
    /// <param name="actualPaymentDue">The calculated actual payment due.</param>
    /// <returns>The mapped CustomerDto.</returns>
    private static CustomerDto MapToDto(Customer customer, decimal estimatedPaymentDue, decimal actualPaymentDue)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            Phone = customer.Phone,
            EstimatedPaymentDue = estimatedPaymentDue,
            ActualPaymentDue = actualPaymentDue
        };
    }
}
