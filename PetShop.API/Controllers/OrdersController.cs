using Microsoft.AspNetCore.Mvc;
using PetShop.Application.DTOs;
using PetShop.Application.Features.Order.Interfaces;

namespace PetShop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class
	OrdersController : ControllerBase
{
	private readonly IOrderService _orderService;

	public OrdersController(IOrderService orderService)
	{
		_orderService = orderService;
	}

	[HttpPost]
	public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
	{
		try
		{
			var order = await _orderService.CreateOrderAsync(createOrderDto);
			return CreatedAtAction(nameof(CreateOrder), new { id = order.Id }, order);
		}
		catch (Exception ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpPost("add-pet")]
	public async Task<ActionResult<OrderDto>> AddOrderPet(CreateOrderPetDto createOrderPetDto)
	{
		try
		{
			var order = await _orderService.AddOrderPetAsync(createOrderPetDto);
			return CreatedAtAction(nameof(AddOrderPet), new { orderId = order.Id }, order);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpDelete("remove-pet")]
	public async Task<ActionResult<OrderDto>> RemoveOrderPet(RemoveOrderPetDto removeOrderPetDto)
	{
		try
		{
			var order = await _orderService.RemoveOrderPetAsync(removeOrderPetDto);
			return Ok(order);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpGet()]
	public async Task<ActionResult<List<OrderDto>>> GetAllOrdersAsync()
	{
		var orders = await _orderService.GetAllOrdersAsync();
		if (orders == null || !orders.Any())
			return NotFound();

		return Ok(orders);
	}

	[HttpGet("customer/{customerId}")]
	public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomer(int customerId)
	{
		try
		{
			var orders = await _orderService.GetOrdersByCustomer(customerId);
			return Ok(orders);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
	}



	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDto updateOrderDto)
	{
		try
		{
			var order = await _orderService.UpdateOrderAsync(id, updateOrderDto);
			if (order == null)
				return NotFound();

			return Ok(order);
		}
		catch (Exception ex)
		{
			return BadRequest(ex.Message);
		}
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetOrder(int id)
	{
		var order = await _orderService.GetOrderAsync(id);
		if (order == null)
			return NotFound();

		return Ok(order);
	}
}