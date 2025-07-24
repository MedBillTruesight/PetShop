using Microsoft.AspNetCore.Mvc;
using PetShop.API.DTOs;
using PetShop.API.Services.Interfaces;
using PetShop.API.DTOs;
using PetShop.API.Models;
using PetShop.API.Services;

namespace PetShop.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderDto dto)
        {
            var order = await _orderService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateOrderDto dto)
        {
            var result = await _orderService.UpdateAsync(id, dto);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}