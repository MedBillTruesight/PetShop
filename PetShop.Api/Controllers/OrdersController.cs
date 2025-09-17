using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PetShop.Api.Dtos;
using PetShop.Api.Enums;
using PetShop.Api.Models;
using PetShop.Api.Services;

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
                return BadRequest(ModelState);
            try
            {
                var orderDto = await _orderService.CreateOrderAsync(request);

                return Created(string.Empty, orderDto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the order");

            }
        }

        [HttpGet("{orderId:guid}")]
        public async Task<IActionResult> GetOrder(Guid orderId)
        {
            try
            {
                var orderDto = await _orderService.GetOrderAsync(orderId);
                if (orderDto == null)
                    return NotFound();
                return Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order");
            }
        }

        [HttpPut("{orderId:guid}")]
        public async Task<IActionResult> UpdateOrder(Guid orderId, [FromBody] UpdateOrderDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var orderDto = await _orderService.UpdateOrderAsync(orderId, request);
                return Ok(orderDto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order");
            }
        }
    }
}
