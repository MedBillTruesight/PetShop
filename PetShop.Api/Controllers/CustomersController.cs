using Microsoft.AspNetCore.Mvc;
using PetShop.Application.DTOs;
using PetShop.Application.Services;

namespace PetShop.Api.Controllers;

/// <summary>
/// Controller for customer-related operations.
/// Provides REST endpoints for creating, retrieving, and updating customers.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/customers")]
public class CustomersController : ControllerBase
{
    private readonly CustomerService _customerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersController"/> class.
    /// </summary>
    /// <param name="customerService">The customer service for business logic operations.</param>
    public CustomersController(CustomerService customerService)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="request">The customer creation request containing first name, last name, email, and phone.</param>
    /// <returns>The created customer with generated ID and payment calculations.</returns>
    /// <response code="201">Customer created successfully.</response>
    /// <response code="400">Validation failure (missing required fields, invalid email format).</response>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        var customer = await _customerService.CreateCustomerAsync(request);
        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customer.Id, version = "1.0" },
            customer);
    }

    /// <summary>
    /// Gets a customer by their unique identifier with calculated payment due amounts.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>The customer with payment calculations.</returns>
    /// <response code="200">Customer found and returned successfully.</response>
    /// <response code="404">Customer with the specified ID does not exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        return Ok(customer);
    }

    /// <summary>
    /// Updates an existing customer (full replacement).
    /// Replaces the entire customer resource with the provided data.
    /// Missing fields are set to null/defaults.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="request">The customer update request containing all customer fields.</param>
    /// <returns>The updated customer with payment calculations.</returns>
    /// <response code="200">Customer updated successfully.</response>
    /// <response code="400">Validation failure (missing required fields, invalid email format).</response>
    /// <response code="404">Customer with the specified ID does not exist.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        var customer = await _customerService.UpdateCustomerAsync(id, request);
        return Ok(customer);
    }

    /// <summary>
    /// Gets all customers with their payment calculations.
    /// </summary>
    /// <returns>A collection of all customers with payment due amounts.</returns>
    /// <response code="200">Customers retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    /// <summary>
    /// Gets all orders for a specific customer with cost calculations.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>A collection of the customer's orders with cost calculations.</returns>
    /// <response code="200">Customer orders retrieved successfully.</response>
    /// <response code="404">Customer with the specified ID does not exist.</response>
    [HttpGet("{id}/orders")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders(Guid id)
    {
        var orders = await _customerService.GetCustomerOrdersAsync(id);
        return Ok(orders);
    }

    /// <summary>
    /// Deletes a customer by their unique identifier.
    /// Business rule: cannot delete if the customer has any orders.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>No content on successful deletion.</returns>
    /// <response code="204">Customer deleted successfully.</response>
    /// <response code="404">Customer with the specified ID does not exist.</response>
    /// <response code="409">Customer has orders and cannot be deleted.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return NoContent();
    }
}
