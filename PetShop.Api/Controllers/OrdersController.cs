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

        [HttpPost]
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

        [HttpGet("{orderId:guid}")]
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

        [HttpPut("{orderId:guid}")]
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
