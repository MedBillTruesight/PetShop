using Microsoft.AspNetCore.Mvc;
using PetShop.Api.Common;
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
        /// Retrieves a customer by ID.
        /// </summary>
        /// <param name="id">The ID of the customer to fetch.</param>
        /// <returns>
        /// <see cref="ApiResponse{T}"/> with <see cref="CustomerDto"/> in Data if found.
        /// Success = false if the customer is not found or an error occurs.
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerDto>> GetCustomer(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid customer ID: {CustomerId}", id);
                    return BadRequest(new ApiResponse<string>(null, "Customer ID must be valid",false));
                }

                _logger.LogInformation("Getting customer with ID: {CustomerId}", id);
                var customer = await _customerService.GetCustomerAsync(id);

                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", id);
                    return NotFound( new ApiResponse<string>(null,"Customer not found",false));
                }

                return Ok(new ApiResponse<CustomerDto>(customer,"Customer fetched successfully", true));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>(null, "An error occurred while retrieving the customer",false));
            }
        }

        /// <summary>
        /// Creates a new customer
        /// </summary>
        /// <param name="request">Customer creation data</param>
        /// <returns>Newly created customer</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CreateCustomerDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {

                    var errMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    _logger.LogWarning("Invalid customer creation request: {ValidationErrors}", errMessage);
                    return BadRequest(new ApiResponse<string>(null, errMessage, false));
                }

                _logger.LogInformation("Creating new customer: {FirstName} {LastName}", request.FirstName, request.LastName);
                var customer = await _customerService.CreateCustomerAsync(request);

                var response = new ApiResponse<CustomerDto>(customer, "Customer created successfully");

                return CreatedAtAction(nameof(GetCustomer),
                    new { id = customer.Id },
                    response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while creating customer");
                return BadRequest(new ApiResponse<string>(null, ex.Message, false));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error while creating customer");
                return BadRequest(new ApiResponse<string>(null, ex.Message, false));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating customer");
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>(null, "An error occurred while creating the customer", false));
            }
        }

        /// <summary>
        /// Updates an existing customer
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="request">Customer update data</param>
        /// <returns>Updated customer</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CustomerDto>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerDto request)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid customer ID: {CustomerId}", id);
                    return BadRequest(new ApiResponse<string>(null, "Customer ID must be valid", false));

                }

                if (!ModelState.IsValid)
                {
                    var errMessage = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    _logger.LogWarning("Invalid customer update request for ID {CustomerId}: {ValidationErrors}",
                        id, errMessage);
                    return BadRequest(new ApiResponse<string>(null, errMessage, false));
                }

                _logger.LogInformation("Updating customer with ID: {CustomerId}", id);
                var customer = await _customerService.UpdateCustomerAsync(id, request);

                if (customer == null)
                {
                    var errMessage = "Customer not found";
                    _logger.LogWarning(errMessage);
                    return NotFound(new ApiResponse<string>(null, errMessage, false));
                }

                return Ok(new ApiResponse<CustomerDto>(customer, "Customer created successfully"));

            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating customer with ID: {CustomerId}", id);
                return BadRequest(new ApiResponse<string>(null, "Validation error while updating customer", false));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argument error while updating customer with ID: {CustomerId}", id);
                return BadRequest(new ApiResponse<string>(null, "Argument error while updating customer", false));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse<string>(null, "An error occurred while updating the customer", false));
            }
        }

    }

}

