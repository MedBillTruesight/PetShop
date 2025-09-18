using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetShop.Api.Common;
using PetShop.Api.Dtos;
using PetShop.Api.Enums;
using PetShop.Api.Models;
using PetShop.Api.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PetShop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;
        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }


        /// <summary>
        /// Creates a new order for a customer.
        /// </summary>
        /// <param name="dto">Order details, including customer ID and pets.</param>
        /// <returns>
        /// <see cref="ApiResponse{T}"/> with the created <see cref="OrderDto"/> in Data.
        /// Success = false if validation fails or an error occurs.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                return BadRequest(new ApiResponse<string>(null, errors, false));
            }

            try
            {
                var orderDto = await _orderService.CreateOrderAsync(request);

                return Created(string.Empty, new ApiResponse<OrderDto>(orderDto, "Order created successfully", true));

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(null, "An error occurred while creating the order", false));
            }
        }
        /// <summary>
        /// Retrieves an order by ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to fetch.</param>
        /// <returns>
        /// <see cref="ApiResponse{T}"/> with <see cref="OrderDto"/> in Data if found.
        /// Success = false if the order is not found or an error occurs.
        /// </returns>
        [HttpGet("{orderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrder(Guid orderId)
        {
            try
            {
                var orderDto = await _orderService.GetOrderAsync(orderId);
                if (orderDto == null)
                    return NotFound(new ApiResponse<string>(null, "Order not found", false));

                return Ok(new ApiResponse<OrderDto>(orderDto, "Order fetched successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<string>(null, "An error occurred while fetching the order", false));

            }
        }

        /// <summary>
        /// Updates an existing order. Allowed changes depend on the current status:
        /// Open: can add/remove pets, change status to Processing if pets exist.
        /// Processing: can update PickupDate, can change status to Delivered.
        /// Delivered: cannot be updated.
        /// </summary>
        /// <param name="orderId">The ID of the order to update.</param>
        /// <param name="dto">The updates to apply.</param>
        /// <returns>
        /// <see cref="ApiResponse{T}"/> with updated <see cref="OrderDto"/> in Data.
        /// </returns>
        [HttpPut("{orderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrder(Guid orderId, [FromBody] UpdateOrderDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                return BadRequest(new ApiResponse<string>(null, errors, false));
            }

            try
            {
                var orderDto = await _orderService.UpdateOrderAsync(orderId, request);
                return Ok(new ApiResponse<OrderDto>(orderDto, "Order updated successfully", true));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<string>(null, ex?.Message, false));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>(null, "An error occurred while updating the order", false));
            }
        }
    }
}
