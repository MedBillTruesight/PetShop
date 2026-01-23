using System.Text.RegularExpressions;

namespace PetShop.Domain;

/// <summary>
/// Represents a customer in the pet shop system.
/// Customers can place multiple orders and have optional contact information.
/// </summary>
public class Customer
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the unique identifier for the customer.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the customer's first name.
    /// </summary>
    public string FirstName { get; private set; }

    /// <summary>
    /// Gets the customer's last name.
    /// </summary>
    public string LastName { get; private set; }

    /// <summary>
    /// Gets the customer's email address, if provided.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Gets the customer's phone number, if provided.
    /// </summary>
    public string? Phone { get; private set; }

    /// <summary>
    /// Gets the collection of orders placed by this customer.
    /// </summary>
    public ICollection<Order> Orders { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Customer"/> class.
    /// </summary>
    /// <param name="firstName">The customer's first name. Cannot be null or empty.</param>
    /// <param name="lastName">The customer's last name. Cannot be null or empty.</param>
    /// <param name="email">The customer's email address. Optional, but must be valid format if provided.</param>
    /// <param name="phone">The customer's phone number. Optional.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when required fields are missing or email format is invalid.</exception>
    public Customer(string firstName, string lastName, string? email = null, string? phone = null)
    {
        ValidateFirstName(firstName);
        ValidateLastName(lastName);
        ValidateEmail(email);

        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        Orders = new List<Order>();
    }

    /// <summary>
    /// Updates the customer's first name.
    /// </summary>
    /// <param name="firstName">The new first name. Cannot be null or empty.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when first name is null or empty.</exception>
    public void UpdateFirstName(string firstName)
    {
        ValidateFirstName(firstName);
        FirstName = firstName;
    }

    /// <summary>
    /// Updates the customer's last name.
    /// </summary>
    /// <param name="lastName">The new last name. Cannot be null or empty.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when last name is null or empty.</exception>
    public void UpdateLastName(string lastName)
    {
        ValidateLastName(lastName);
        LastName = lastName;
    }

    /// <summary>
    /// Updates the customer's email address.
    /// </summary>
    /// <param name="email">The new email address. Optional, but must be valid format if provided.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when email format is invalid.</exception>
    public void UpdateEmail(string? email)
    {
        ValidateEmail(email);
        Email = email;
    }

    /// <summary>
    /// Updates the customer's phone number.
    /// </summary>
    /// <param name="phone">The new phone number. Optional.</param>
    public void UpdatePhone(string? phone)
    {
        Phone = phone;
    }

    private static void ValidateFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new BusinessRuleViolationException("First name is required and cannot be empty.");
        }
    }

    private static void ValidateLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new BusinessRuleViolationException("Last name is required and cannot be empty.");
        }
    }

    private static void ValidateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return; // Email is optional
        }

        if (!EmailRegex.IsMatch(email))
        {
            throw new BusinessRuleViolationException($"Invalid email format: {email}");
        }
    }
}
