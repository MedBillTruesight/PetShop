using FluentAssertions;
using PetShop.Domain;

namespace PetShop.Tests;

/// <summary>
/// Unit tests for the Pet domain entity.
/// </summary>
public class PetTests
{
    private static readonly Guid ValidOrderId = Guid.NewGuid();

    [Fact]
    public void Constructor_WithValidData_ShouldCreatePet()
    {
        // Arrange
        const string name = "Fluffy";
        const decimal price = 100.50m;

        // Act
        var pet = new Pet(ValidOrderId, name, price);

        // Assert
        pet.Should().NotBeNull();
        pet.Id.Should().NotBeEmpty();
        pet.OrderId.Should().Be(ValidOrderId);
        pet.Name.Should().Be(name);
        pet.Price.Should().Be(price);
        pet.Kind.Should().BeNull();
        pet.Color.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithValidKind_ShouldCreatePet()
    {
        // Arrange
        const string name = "Max";
        const decimal price = 200m;
        const string kind = "Dog";

        // Act
        var pet = new Pet(ValidOrderId, name, price, kind);

        // Assert
        pet.Should().NotBeNull();
        pet.Kind.Should().Be(kind);
    }

    [Fact]
    public void Constructor_WithValidColor_ShouldCreatePet()
    {
        // Arrange
        const string name = "Whiskers";
        const decimal price = 150m;
        const string color = "Brown";

        // Act
        var pet = new Pet(ValidOrderId, name, price, color: color);

        // Assert
        pet.Should().NotBeNull();
        pet.Color.Should().Be(color);
    }

    [Fact]
    public void Constructor_WithAllValidData_ShouldCreatePet()
    {
        // Arrange
        const string name = "Buddy";
        const decimal price = 300m;
        const string kind = "Cat";
        const string color = "White";

        // Act
        var pet = new Pet(ValidOrderId, name, price, kind, color);

        // Assert
        pet.Should().NotBeNull();
        pet.Name.Should().Be(name);
        pet.Price.Should().Be(price);
        pet.Kind.Should().Be(kind);
        pet.Color.Should().Be(color);
    }

    [Fact]
    public void Constructor_WithEmptyOrderId_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var emptyOrderId = Guid.Empty;
        const string name = "Fluffy";
        const decimal price = 100m;

        // Act
        var act = () => new Pet(emptyOrderId, name, price);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Order ID is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string? name = null;
        const decimal price = 100m;

        // Act
        var act = () => new Pet(ValidOrderId, name!, price);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Pet name is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string name = "";
        const decimal price = 100m;

        // Act
        var act = () => new Pet(ValidOrderId, name, price);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Pet name is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string name = "   ";
        const decimal price = 100m;

        // Act
        var act = () => new Pet(ValidOrderId, name, price);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Pet name is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithZeroPrice_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string name = "Fluffy";
        const decimal price = 0m;

        // Act
        var act = () => new Pet(ValidOrderId, name, price);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage($"Pet price must be greater than zero. Provided value: {price}");
    }

    [Fact]
    public void Constructor_WithNegativePrice_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string name = "Fluffy";
        const decimal price = -10m;

        // Act
        var act = () => new Pet(ValidOrderId, name, price);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage($"Pet price must be greater than zero. Provided value: {price}");
    }

    [Fact]
    public void Constructor_WithVerySmallPositivePrice_ShouldCreatePet()
    {
        // Arrange
        const string name = "Tiny";
        const decimal price = 0.01m;

        // Act
        var pet = new Pet(ValidOrderId, name, price);

        // Assert
        pet.Should().NotBeNull();
        pet.Price.Should().Be(price);
    }

    [Fact]
    public void Constructor_WithNullKind_ShouldAcceptNullKind()
    {
        // Arrange
        const string name = "Fluffy";
        const decimal price = 100m;
        const string? kind = null;

        // Act
        var pet = new Pet(ValidOrderId, name, price, kind);

        // Assert
        pet.Should().NotBeNull();
        pet.Kind.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNullColor_ShouldAcceptNullColor()
    {
        // Arrange
        const string name = "Fluffy";
        const decimal price = 100m;
        const string? color = null;

        // Act
        var pet = new Pet(ValidOrderId, name, price, color: color);

        // Assert
        pet.Should().NotBeNull();
        pet.Color.Should().BeNull();
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m);
        const string newName = "Max";

        // Act
        pet.UpdateName(newName);

        // Assert
        pet.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_WithNull_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m);

        // Act
        var act = () => pet.UpdateName(null!);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Pet name is required and cannot be empty.");
    }

    [Fact]
    public void UpdateName_WithEmpty_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m);

        // Act
        var act = () => pet.UpdateName("");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Pet name is required and cannot be empty.");
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m);
        const decimal newPrice = 200m;

        // Act
        pet.UpdatePrice(newPrice);

        // Assert
        pet.Price.Should().Be(newPrice);
    }

    [Fact]
    public void UpdatePrice_WithZero_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m);

        // Act
        var act = () => pet.UpdatePrice(0m);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Pet price must be greater than zero. Provided value: 0");
    }

    [Fact]
    public void UpdatePrice_WithNegative_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m);

        // Act
        var act = () => pet.UpdatePrice(-50m);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Pet price must be greater than zero. Provided value: -50");
    }

    [Fact]
    public void UpdateKind_WithValidKind_ShouldUpdateKind()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m);
        const string newKind = "Dog";

        // Act
        pet.UpdateKind(newKind);

        // Assert
        pet.Kind.Should().Be(newKind);
    }

    [Fact]
    public void UpdateKind_WithNull_ShouldAcceptNull()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m, "Cat");

        // Act
        pet.UpdateKind(null);

        // Assert
        pet.Kind.Should().BeNull();
    }

    [Fact]
    public void UpdateColor_WithValidColor_ShouldUpdateColor()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m);
        const string newColor = "Brown";

        // Act
        pet.UpdateColor(newColor);

        // Assert
        pet.Color.Should().Be(newColor);
    }

    [Fact]
    public void UpdateColor_WithNull_ShouldAcceptNull()
    {
        // Arrange
        var pet = new Pet(ValidOrderId, "Fluffy", 100m, color: "White");

        // Act
        pet.UpdateColor(null);

        // Assert
        pet.Color.Should().BeNull();
    }
}
