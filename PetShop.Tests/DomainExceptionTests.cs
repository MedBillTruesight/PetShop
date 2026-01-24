using FluentAssertions;
using PetShop.Domain;

namespace PetShop.Tests;

/// <summary>
/// Unit tests for domain exception classes to achieve 100% coverage.
/// Tests all constructors and exception behavior.
/// </summary>
public class DomainExceptionTests
{
    #region BusinessRuleViolationException Tests

    [Fact]
    public void BusinessRuleViolationException_DefaultConstructor_CreatesInstance()
    {
        // Act
        var exception = new BusinessRuleViolationException();

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<BusinessRuleViolationException>();
        exception.Message.Should().Contain("PetShop.Domain.BusinessRuleViolationException");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void BusinessRuleViolationException_MessageConstructor_CreatesInstanceWithMessage()
    {
        // Arrange
        const string expectedMessage = "Business rule violated";

        // Act
        var exception = new BusinessRuleViolationException(expectedMessage);

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<BusinessRuleViolationException>();
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void BusinessRuleViolationException_MessageAndInnerExceptionConstructor_CreatesInstanceWithBoth()
    {
        // Arrange
        const string expectedMessage = "Business rule violated";
        var innerException = new ArgumentException("Invalid argument");

        // Act
        var exception = new BusinessRuleViolationException(expectedMessage, innerException);

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<BusinessRuleViolationException>();
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void BusinessRuleViolationException_InheritsFromException()
    {
        // Act
        var exception = new BusinessRuleViolationException();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    #endregion

    #region InvalidOrderStateException Tests

    [Fact]
    public void InvalidOrderStateException_DefaultConstructor_CreatesInstance()
    {
        // Act
        var exception = new InvalidOrderStateException();

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<InvalidOrderStateException>();
        exception.Message.Should().Contain("PetShop.Domain.InvalidOrderStateException");
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void InvalidOrderStateException_MessageConstructor_CreatesInstanceWithMessage()
    {
        // Arrange
        const string expectedMessage = "Invalid order state transition";

        // Act
        var exception = new InvalidOrderStateException(expectedMessage);

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<InvalidOrderStateException>();
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().BeNull();
    }

    [Fact]
    public void InvalidOrderStateException_MessageAndInnerExceptionConstructor_CreatesInstanceWithBoth()
    {
        // Arrange
        const string expectedMessage = "Invalid order state transition";
        var innerException = new InvalidOperationException("Operation not allowed");

        // Act
        var exception = new InvalidOrderStateException(expectedMessage, innerException);

        // Assert
        exception.Should().NotBeNull();
        exception.Should().BeOfType<InvalidOrderStateException>();
        exception.Message.Should().Be(expectedMessage);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void InvalidOrderStateException_InheritsFromException()
    {
        // Act
        var exception = new InvalidOrderStateException();

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    #endregion

    #region Integration Tests with Domain Entities

    [Fact]
    public void Customer_ThrowsBusinessRuleViolationException_WithMessageConstructor()
    {
        // Act & Assert
        var act = () => new Customer("", "Doe");
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("First name is required and cannot be empty.");
    }

    [Fact]
    public void Order_ThrowsInvalidOrderStateException_WithMessageConstructor()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today.AddDays(1)));
        var pet = new Pet(order.Id, "TestPet", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        // Act & Assert - Try to transition from Delivered (should fail)
        var act = () => order.TransitionToProcessing();
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage("Cannot transition from Delivered to Processing. Order must be in Open status.");
    }

    [Fact]
    public void Pet_ThrowsBusinessRuleViolationException_WithMessageConstructor()
    {
        // Act & Assert
        var act = () => new Pet(Guid.NewGuid(), "", 100m);
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Pet name is required and cannot be empty.");
    }

    #endregion
}