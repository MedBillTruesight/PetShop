using Microsoft.AspNetCore.Mvc;
using PetShop.Api.Dtos;
using PetShop.Api.Services;
using System.ComponentModel.DataAnnotations;

namespace PetShop.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a specific customer by ID
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns>Customer details with payment information</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid customer ID: {CustomerId}", id);
                    return BadRequest("Customer ID must be greater than 0");
                }

                _logger.LogInformation("Getting customer with ID: {CustomerId}", id);
                var customer = await _customerService.GetCustomerAsync(id);

                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", id);
                    return NotFound();
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the customer");
            }
        }

        /// <summary>
        /// Creates a new customer
        /// </summary>
        /// <param name="request">Customer creation data</param>
        /// <returns>Newly created customer</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid customer creation request: {ValidationErrors}",
                        string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating new customer: {FirstName} {LastName}", request.FirstName, request.LastName);
                var customer = await _customerService.CreateCustomerAsync(request);

                return CreatedAtAction(
                    nameof(GetCustomer),
                    new { id = customer.Id },
                    customer);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating customer");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error while creating customer");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating customer");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the customer");
            }
        }

        /// <summary>
        /// Updates an existing customer
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="request">Customer update data</param>
        /// <returns>Updated customer</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerDto>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerDto request)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid customer ID: {CustomerId}", id);
                    return BadRequest("Customer ID must be greater than 0");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid customer update request for ID {CustomerId}: {ValidationErrors}",
                        id, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating customer with ID: {CustomerId}", id);
                var customer = await _customerService.UpdateCustomerAsync(id, request);

                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found for update", id);
                    return NotFound();
                }

                return Ok(customer);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating customer with ID: {CustomerId}", id);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error while updating customer with ID: {CustomerId}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the customer");
            }
        }

    }

}

