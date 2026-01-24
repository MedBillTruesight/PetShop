using Microsoft.AspNetCore.Mvc;
using PetShop.Api.DTOs;
using PetShop.Application.DTOs;
using PetShop.Application.Services;
using PetShop.Domain;

namespace PetShop.Api.Controllers;

/// <summary>
/// Controller for order-related operations.
/// Provides REST endpoints for creating, retrieving, updating orders, state transitions, and pet management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="orderService">The order service for business logic operations.</param>
    public OrdersController(OrderService orderService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="request">The order creation request containing customer ID and pickup date.</param>
    /// <returns>The created order with generated ID and default status Open.</returns>
    /// <response code="201">Order created successfully.</response>
    /// <response code="400">Validation failure (missing required fields, invalid pickup date).</response>
    /// <response code="404">Customer with the specified ID does not exist.</response>
    /// <response code="422">Semantic validation failure (e.g., pickup date in the past).</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        var order = await _orderService.CreateOrderAsync(request);
        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id, version = "1.0" },
            order);
    }

    /// <summary>
    /// Gets an order by its unique identifier with calculated cost.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The order with cost calculation based on status.</returns>
    /// <response code="200">Order found and returned successfully.</response>
    /// <response code="404">Order with the specified ID does not exist.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        return Ok(order);
    }

    /// <summary>
    /// Updates an existing order's pickup date (partial update, status-aware).
    /// Only pickup date can be updated. Modifications are restricted by order status.
    /// </summary>
    /// <param name="id">The unique identifier of the order to update.</param>
    /// <param name="request">The order update request containing the new pickup date.</param>
    /// <returns>The updated order with cost calculations.</returns>
    /// <response code="200">Order updated successfully.</response>
    /// <response code="400">Validation failure (missing required fields, invalid pickup date).</response>
    /// <response code="404">Order with the specified ID does not exist.</response>
    /// <response code="409">Order is in Delivered status and cannot be modified.</response>
    /// <response code="422">Semantic validation failure (e.g., pickup date in the past).</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> UpdateOrder(Guid id, [FromBody] UpdateOrderRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        var order = await _orderService.UpdateOrderAsync(id, request);
        return Ok(order);
    }

    /// <summary>
    /// Transitions an order to a specified status.
    /// Valid transitions: Open → Processing, Processing → Delivered.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <param name="request">The transition request containing the target status.</param>
    /// <returns>The updated order with cost calculations.</returns>
    /// <response code="200">Order transitioned successfully.</response>
    /// <response code="400">Validation failure (missing required fields).</response>
    /// <response code="404">Order with the specified ID does not exist.</response>
    /// <response code="409">Invalid state transition (e.g., order has no pets, or invalid transition path).</response>
    /// <response code="422">Semantic validation failure (e.g., pickup date in the past).</response>
    [HttpPost("{id}/transition")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<OrderDto>> TransitionOrder(Guid id, [FromBody] TransitionOrderRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        OrderDto order;
        if (request.Status == OrderStatus.Processing)
        {
            order = await _orderService.TransitionOrderToProcessingAsync(id);
        }
        else if (request.Status == OrderStatus.Delivered)
        {
            order = await _orderService.TransitionOrderToDeliveredAsync(id);
        }
        else
        {
            return BadRequest($"Invalid target status. Valid transitions are to Processing or Delivered.");
        }

        return Ok(order);
    }

    /// <summary>
    /// Adds a pet to an order.
    /// Only allowed when order status is Open.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <param name="request">The pet creation request containing name, price, kind, and color.</param>
    /// <returns>The updated order with the new pet included.</returns>
    /// <response code="201">Pet added successfully. Returns updated order.</response>
    /// <response code="400">Validation failure (missing required fields, invalid price).</response>
    /// <response code="404">Order with the specified ID does not exist.</response>
    /// <response code="409">Order is not in Open status and cannot be modified.</response>
    [HttpPost("{id}/pets")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> AddPetToOrder(Guid id, [FromBody] AddPetRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body cannot be null.");
        }

        var order = await _orderService.AddPetToOrderAsync(id, request);
        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id, version = "1.0" },
            order);
    }

    /// <summary>
    /// Removes a pet from an order.
    /// Only allowed when order status is Open.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <param name="petId">The unique identifier of the pet to remove.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Pet removed successfully.</response>
    /// <response code="404">Order or pet with the specified ID does not exist.</response>
    /// <response code="409">Order is not in Open status and cannot be modified.</response>
    [HttpDelete("{id}/pets/{petId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemovePetFromOrder(Guid id, Guid petId)
    {
        await _orderService.RemovePetFromOrderAsync(id, petId);
        return NoContent();
    }
}
