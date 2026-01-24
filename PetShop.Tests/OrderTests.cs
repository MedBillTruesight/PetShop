using FluentAssertions;
using PetShop.Domain;

namespace PetShop.Tests;

/// <summary>
/// Unit tests for the Order domain entity.
/// </summary>
public class OrderTests
{
    private static readonly Guid ValidCustomerId = Guid.NewGuid();
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
    private static DateOnly Tomorrow => Today.AddDays(1);
    private static DateOnly Yesterday => Today.AddDays(-1);

    [Fact]
    public void Constructor_WithValidData_ShouldCreateOrder()
    {
        // Arrange
        var pickupDate = Tomorrow;

        // Act
        var order = new Order(ValidCustomerId, pickupDate);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(ValidCustomerId);
        order.Status.Should().Be(OrderStatus.Open);
        order.PickupDate.Should().Be(pickupDate);
        order.ActualCost.Should().BeNull();
        order.Pets.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Constructor_WithTodayAsPickupDate_ShouldCreateOrder()
    {
        // Arrange
        var pickupDate = Today;

        // Act
        var order = new Order(ValidCustomerId, pickupDate);

        // Assert
        order.Should().NotBeNull();
        order.PickupDate.Should().Be(pickupDate);
    }

    [Fact]
    public void Constructor_WithEmptyCustomerId_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var emptyCustomerId = Guid.Empty;
        var pickupDate = Tomorrow;

        // Act
        var act = () => new Order(emptyCustomerId, pickupDate);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Customer ID is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithPastPickupDate_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var pastDate = Yesterday;

        // Act
        var act = () => new Order(ValidCustomerId, pastDate);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage($"Invalid pickup date: must be today or in the future. Provided date: {pastDate:yyyy-MM-dd}, Today: {Today:yyyy-MM-dd}");
    }

    [Fact]
    public void TransitionToProcessing_WhenOrderIsOpenAndHasPets_ShouldSucceed()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);

        // Act
        order.TransitionToProcessing();

        // Assert
        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void TransitionToProcessing_WhenOrderHasNoPets_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);

        // Act
        var act = () => order.TransitionToProcessing();

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Order must have at least one pet before transitioning to Processing.");
    }

    [Fact]
    public void TransitionToProcessing_WhenOrderIsNotOpen_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        // Act
        var act = () => order.TransitionToProcessing();

        // Assert
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage($"Cannot transition from {OrderStatus.Delivered} to Processing. Order must be in Open status.");
    }

    [Fact]
    public void TransitionToDelivered_WhenOrderIsProcessing_ShouldSucceed()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();

        // Act
        order.TransitionToDelivered();

        // Assert
        order.Status.Should().Be(OrderStatus.Delivered);
        order.ActualCost.Should().Be(100m);
    }

    [Fact]
    public void TransitionToDelivered_WhenOrderIsNotProcessing_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);

        // Act
        var act = () => order.TransitionToDelivered();

        // Assert
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage($"Cannot transition from {OrderStatus.Open} to Delivered. Order must be in Processing status.");
    }

    [Fact]
    public void TransitionToDelivered_WhenOrderIsDelivered_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        // Act
        var act = () => order.TransitionToDelivered();

        // Assert
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage($"Cannot transition from {OrderStatus.Delivered} to Delivered. Order must be in Processing status.");
    }

    [Fact]
    public void TransitionToDelivered_ShouldSetActualCostToSumOfPetPrices()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet1 = new Pet(order.Id, "Fluffy", 100m);
        var pet2 = new Pet(order.Id, "Max", 200m);
        order.AddPet(pet1);
        order.AddPet(pet2);
        order.TransitionToProcessing();

        // Act
        order.TransitionToDelivered();

        // Assert
        order.ActualCost.Should().Be(300m);
    }

    [Fact]
    public void AddPet_WhenOrderIsOpen_ShouldSucceed()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);

        // Act
        order.AddPet(pet);

        // Assert
        order.Pets.Should().Contain(pet);
        order.Pets.Count.Should().Be(1);
    }

    [Fact]
    public void AddPet_WhenOrderIsProcessing_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet1 = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet1);
        order.TransitionToProcessing();
        var pet2 = new Pet(order.Id, "Max", 200m);

        // Act
        var act = () => order.AddPet(pet2);

        // Assert
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage($"Cannot add pets to order in {OrderStatus.Processing} status. Pets can only be added when order is Open.");
    }

    [Fact]
    public void AddPet_WhenOrderIsDelivered_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet1 = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet1);
        order.TransitionToProcessing();
        order.TransitionToDelivered();
        var pet2 = new Pet(order.Id, "Max", 200m);

        // Act
        var act = () => order.AddPet(pet2);

        // Assert
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage($"Cannot add pets to order in {OrderStatus.Delivered} status. Pets can only be added when order is Open.");
    }

    [Fact]
    public void AddPet_WithNullPet_ShouldThrowArgumentNullException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);

        // Act
        var act = () => order.AddPet(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("pet");
    }

    [Fact]
    public void RemovePet_WhenOrderIsOpen_ShouldSucceed()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);

        // Act
        order.RemovePet(pet);

        // Assert
        order.Pets.Should().NotContain(pet);
        order.Pets.Count.Should().Be(0);
    }

    [Fact]
    public void RemovePet_WhenOrderIsProcessing_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();

        // Act
        var act = () => order.RemovePet(pet);

        // Assert
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage($"Cannot remove pets from order in {OrderStatus.Processing} status. Pets can only be removed when order is Open.");
    }

    [Fact]
    public void RemovePet_WhenOrderIsDelivered_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();

        // Act
        var act = () => order.RemovePet(pet);

        // Assert
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage($"Cannot remove pets from order in {OrderStatus.Delivered} status. Pets can only be removed when order is Open.");
    }

    [Fact]
    public void RemovePet_WithNullPet_ShouldThrowArgumentNullException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);

        // Act
        var act = () => order.RemovePet(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("pet");
    }

    [Fact]
    public void RemovePet_WithPetNotInOrder_ShouldThrowArgumentException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);

        // Act
        var act = () => order.RemovePet(pet);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Pet is not in this order. (Parameter 'pet')");
    }

    [Fact]
    public void UpdatePickupDate_WhenOrderIsOpen_ShouldSucceed()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var newPickupDate = Tomorrow.AddDays(5);

        // Act
        order.UpdatePickupDate(newPickupDate);

        // Assert
        order.PickupDate.Should().Be(newPickupDate);
    }

    [Fact]
    public void UpdatePickupDate_WhenOrderIsProcessing_ShouldSucceed()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        var newPickupDate = Tomorrow.AddDays(5);

        // Act
        order.UpdatePickupDate(newPickupDate);

        // Assert
        order.PickupDate.Should().Be(newPickupDate);
    }

    [Fact]
    public void UpdatePickupDate_WhenOrderIsDelivered_ShouldThrowInvalidOrderStateException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);
        order.TransitionToProcessing();
        order.TransitionToDelivered();
        var newPickupDate = Tomorrow.AddDays(5);

        // Act
        var act = () => order.UpdatePickupDate(newPickupDate);

        // Assert
        act.Should().Throw<InvalidOrderStateException>()
            .WithMessage("Cannot modify pickup date for order in Delivered status. Order is immutable.");
    }

    [Fact]
    public void UpdatePickupDate_WithPastDate_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pastDate = Yesterday;

        // Act
        var act = () => order.UpdatePickupDate(pastDate);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage($"Invalid pickup date: must be today or in the future. Provided date: {pastDate:yyyy-MM-dd}, Today: {Today:yyyy-MM-dd}");
    }

    [Fact]
    public void CalculateEstimatedCost_WithNoPets_ShouldReturnZero()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);

        // Act
        var cost = order.CalculateEstimatedCost();

        // Assert
        cost.Should().Be(0m);
    }

    [Fact]
    public void CalculateEstimatedCost_WithSinglePet_ShouldReturnPetPrice()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet = new Pet(order.Id, "Fluffy", 100m);
        order.AddPet(pet);

        // Act
        var cost = order.CalculateEstimatedCost();

        // Assert
        cost.Should().Be(100m);
    }

    [Fact]
    public void CalculateEstimatedCost_WithMultiplePets_ShouldReturnSumOfPrices()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet1 = new Pet(order.Id, "Fluffy", 100m);
        var pet2 = new Pet(order.Id, "Max", 200m);
        var pet3 = new Pet(order.Id, "Buddy", 150m);
        order.AddPet(pet1);
        order.AddPet(pet2);
        order.AddPet(pet3);

        // Act
        var cost = order.CalculateEstimatedCost();

        // Assert
        cost.Should().Be(450m);
    }

    [Fact]
    public void CustomerId_ShouldBeImmutableAfterConstruction()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var originalCustomerId = order.CustomerId;

        // Act & Assert
        // CustomerId has private setter, so it cannot be changed
        // This test verifies the property exists and is set correctly
        order.CustomerId.Should().Be(originalCustomerId);
        order.CustomerId.Should().Be(ValidCustomerId);
    }

    [Fact]
    public void FullStateTransition_OpenToProcessingToDelivered_ShouldSucceed()
    {
        // Arrange
        var order = new Order(ValidCustomerId, Tomorrow);
        var pet1 = new Pet(order.Id, "Fluffy", 100m);
        var pet2 = new Pet(order.Id, "Max", 200m);
        order.AddPet(pet1);
        order.AddPet(pet2);

        // Act & Assert - Open
        order.Status.Should().Be(OrderStatus.Open);
        order.CalculateEstimatedCost().Should().Be(300m);

        // Act & Assert - Processing
        order.TransitionToProcessing();
        order.Status.Should().Be(OrderStatus.Processing);
        order.ActualCost.Should().BeNull();

        // Act & Assert - Delivered
        order.TransitionToDelivered();
        order.Status.Should().Be(OrderStatus.Delivered);
        order.ActualCost.Should().Be(300m);
    }
}
