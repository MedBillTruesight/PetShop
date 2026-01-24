using Microsoft.AspNetCore.Mvc;
using PetShop.Domain;
using System.ComponentModel.DataAnnotations;

namespace PetShop.Api.Controllers;

/// <summary>
/// Test controller for verifying global error handling.
/// This controller should only be used in test scenarios.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/error-test")]
public class ErrorTestController : ControllerBase
{
    /// <summary>
    /// Test endpoint that throws KeyNotFoundException.
    /// </summary>
    [HttpGet("not-found")]
    public IActionResult ThrowNotFound()
    {
        throw new KeyNotFoundException("Customer not found");
    }

    /// <summary>
    /// Test endpoint that throws InvalidOrderStateException.
    /// </summary>
    [HttpGet("invalid-state")]
    public IActionResult ThrowInvalidState()
    {
        throw new InvalidOrderStateException("Cannot transition order from Delivered to Processing");
    }

    /// <summary>
    /// Test endpoint that throws BusinessRuleViolationException (409 Conflict).
    /// </summary>
    [HttpGet("business-rule-conflict")]
    public IActionResult ThrowBusinessRuleConflict()
    {
        throw new BusinessRuleViolationException("Order cannot exceed maximum number of pets");
    }

    /// <summary>
    /// Test endpoint that throws BusinessRuleViolationException (422 Unprocessable Entity).
    /// </summary>
    [HttpGet("business-rule-unprocessable")]
    public IActionResult ThrowBusinessRuleUnprocessable()
    {
        throw new BusinessRuleViolationException("Pickup date cannot be in the past");
    }

    /// <summary>
    /// Test endpoint that throws ArgumentNullException.
    /// </summary>
    [HttpGet("argument-null")]
    public IActionResult ThrowArgumentNull()
    {
        throw new ArgumentNullException(nameof(Request));
    }

    /// <summary>
    /// Test endpoint that throws ArgumentException.
    /// </summary>
    [HttpGet("argument")]
    public IActionResult ThrowArgument()
    {
        throw new ArgumentException("Invalid argument value", "customerId");
    }

    /// <summary>
    /// Test endpoint that throws an unhandled exception (500 Internal Server Error).
    /// </summary>
    [HttpGet("unhandled")]
    public IActionResult ThrowUnhandled()
    {
        throw new InvalidOperationException("An unexpected error occurred");
    }
}
