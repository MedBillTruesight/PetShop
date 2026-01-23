using FluentAssertions;
using PetShop.Domain;

namespace PetShop.Tests;

/// <summary>
/// Unit tests for the Customer domain entity.
/// </summary>
public class CustomerTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "Doe";

        // Act
        var customer = new Customer(firstName, lastName);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().NotBeEmpty();
        customer.FirstName.Should().Be(firstName);
        customer.LastName.Should().Be(lastName);
        customer.Email.Should().BeNull();
        customer.Phone.Should().BeNull();
        customer.Orders.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateCustomer()
    {
        // Arrange
        const string firstName = "Jane";
        const string lastName = "Smith";
        const string email = "jane.smith@example.com";

        // Act
        var customer = new Customer(firstName, lastName, email);

        // Assert
        customer.Should().NotBeNull();
        customer.Email.Should().Be(email);
    }

    [Fact]
    public void Constructor_WithValidPhone_ShouldCreateCustomer()
    {
        // Arrange
        const string firstName = "Bob";
        const string lastName = "Johnson";
        const string phone = "555-1234";

        // Act
        var customer = new Customer(firstName, lastName, phone: phone);

        // Assert
        customer.Should().NotBeNull();
        customer.Phone.Should().Be(phone);
    }

    [Fact]
    public void Constructor_WithAllValidData_ShouldCreateCustomer()
    {
        // Arrange
        const string firstName = "Alice";
        const string lastName = "Williams";
        const string email = "alice@example.com";
        const string phone = "555-5678";

        // Act
        var customer = new Customer(firstName, lastName, email, phone);

        // Assert
        customer.Should().NotBeNull();
        customer.FirstName.Should().Be(firstName);
        customer.LastName.Should().Be(lastName);
        customer.Email.Should().Be(email);
        customer.Phone.Should().Be(phone);
    }

    [Fact]
    public void Constructor_WithNullFirstName_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string? firstName = null;
        const string lastName = "Doe";

        // Act
        var act = () => new Customer(firstName!, lastName);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("First name is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithEmptyFirstName_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string firstName = "";
        const string lastName = "Doe";

        // Act
        var act = () => new Customer(firstName, lastName);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("First name is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithWhitespaceFirstName_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string firstName = "   ";
        const string lastName = "Doe";

        // Act
        var act = () => new Customer(firstName, lastName);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("First name is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithNullLastName_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string firstName = "John";
        const string? lastName = null;

        // Act
        var act = () => new Customer(firstName, lastName!);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Last name is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithEmptyLastName_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "";

        // Act
        var act = () => new Customer(firstName, lastName);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Last name is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithWhitespaceLastName_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "   ";

        // Act
        var act = () => new Customer(firstName, lastName);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Last name is required and cannot be empty.");
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldAcceptNullEmail()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "Doe";
        const string? email = null;

        // Act
        var customer = new Customer(firstName, lastName, email);

        // Assert
        customer.Should().NotBeNull();
        customer.Email.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ShouldAcceptEmptyEmail()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "Doe";
        const string email = "";

        // Act
        var customer = new Customer(firstName, lastName, email);

        // Assert
        customer.Should().NotBeNull();
        customer.Email.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithValidEmailFormats_ShouldAcceptValidEmails()
    {
        // Arrange
        var validEmails = new[]
        {
            "test@example.com",
            "user.name@example.co.uk",
            "user+tag@example.com",
            "user123@test-domain.com"
        };

        foreach (var email in validEmails)
        {
            // Act
            var customer = new Customer("John", "Doe", email);

            // Assert
            customer.Email.Should().Be(email, $"because {email} is a valid email format");
        }
    }

    [Fact]
    public void Constructor_WithInvalidEmailFormat_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "Doe";
        const string invalidEmail = "not-an-email";

        // Act
        var act = () => new Customer(firstName, lastName, invalidEmail);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage($"Invalid email format: {invalidEmail}");
    }

    [Fact]
    public void Constructor_WithInvalidEmailFormats_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var invalidEmails = new[]
        {
            "missing-at-sign.com",
            "@missing-domain.com",
            "missing-tld@example",
            "spaces in@email.com",
            "missing@.com"
        };

        foreach (var email in invalidEmails)
        {
            // Act
            var act = () => new Customer("John", "Doe", email);

            // Assert
            act.Should().Throw<BusinessRuleViolationException>()
                .WithMessage($"Invalid email format: {email}", $"because {email} is an invalid email format");
        }
    }

    [Fact]
    public void Constructor_WithNullPhone_ShouldAcceptNullPhone()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "Doe";
        const string? phone = null;

        // Act
        var customer = new Customer(firstName, lastName, phone: phone);

        // Assert
        customer.Should().NotBeNull();
        customer.Phone.Should().BeNull();
    }

    [Fact]
    public void UpdateFirstName_WithValidName_ShouldUpdateFirstName()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        const string newFirstName = "Jane";

        // Act
        customer.UpdateFirstName(newFirstName);

        // Assert
        customer.FirstName.Should().Be(newFirstName);
    }

    [Fact]
    public void UpdateFirstName_WithNull_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");

        // Act
        var act = () => customer.UpdateFirstName(null!);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("First name is required and cannot be empty.");
    }

    [Fact]
    public void UpdateLastName_WithValidName_ShouldUpdateLastName()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        const string newLastName = "Smith";

        // Act
        customer.UpdateLastName(newLastName);

        // Assert
        customer.LastName.Should().Be(newLastName);
    }

    [Fact]
    public void UpdateLastName_WithNull_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");

        // Act
        var act = () => customer.UpdateLastName(null!);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Last name is required and cannot be empty.");
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateEmail()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        const string newEmail = "newemail@example.com";

        // Act
        customer.UpdateEmail(newEmail);

        // Assert
        customer.Email.Should().Be(newEmail);
    }

    [Fact]
    public void UpdateEmail_WithNull_ShouldAcceptNull()
    {
        // Arrange
        var customer = new Customer("John", "Doe", "old@example.com");

        // Act
        customer.UpdateEmail(null);

        // Assert
        customer.Email.Should().BeNull();
    }

    [Fact]
    public void UpdateEmail_WithInvalidEmail_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        const string invalidEmail = "invalid-email";

        // Act
        var act = () => customer.UpdateEmail(invalidEmail);

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage($"Invalid email format: {invalidEmail}");
    }

    [Fact]
    public void UpdatePhone_WithValidPhone_ShouldUpdatePhone()
    {
        // Arrange
        var customer = new Customer("John", "Doe");
        const string newPhone = "555-9999";

        // Act
        customer.UpdatePhone(newPhone);

        // Assert
        customer.Phone.Should().Be(newPhone);
    }

    [Fact]
    public void UpdatePhone_WithNull_ShouldAcceptNull()
    {
        // Arrange
        var customer = new Customer("John", "Doe", phone: "555-1234");

        // Act
        customer.UpdatePhone(null);

        // Assert
        customer.Phone.Should().BeNull();
    }
}
