using PetShop.Domain;

/// <summary>
/// Fluent builder for creating Customer domain entities in tests.
/// Provides a clean, readable API for constructing test data.
/// </summary>
public class CustomerBuilder
{
    private string _firstName = "John";
    private string _lastName = "Doe";
    private string? _email;
    private string? _phone;

    public CustomerBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public CustomerBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public CustomerBuilder WithEmail(string? email)
    {
        _email = email;
        return this;
    }

    public CustomerBuilder WithPhone(string? phone)
    {
        _phone = phone;
        return this;
    }

    public CustomerBuilder WithCompleteInfo()
    {
        _firstName = "Jane";
        _lastName = "Smith";
        _email = "jane.smith@example.com";
        _phone = "555-1234";
        return this;
    }

    public CustomerBuilder WithMinimalInfo()
    {
        _firstName = "Bob";
        _lastName = "Johnson";
        _email = null;
        _phone = null;
        return this;
    }

    public Customer Build()
    {
        return new Customer(_firstName, _lastName, _email, _phone);
    }
}