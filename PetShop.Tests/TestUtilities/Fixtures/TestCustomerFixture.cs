using PetShop.Domain;

/// <summary>
/// Test fixture providing pre-configured Customer entities for common test scenarios.
/// </summary>
public class TestCustomerFixture
{
    public Customer BasicCustomer => new CustomerBuilder()
        .WithFirstName("John")
        .WithLastName("Doe")
        .Build();

    public Customer CustomerWithEmail => new CustomerBuilder()
        .WithCompleteInfo()
        .Build();

    public Customer CustomerWithPhone => new CustomerBuilder()
        .WithFirstName("Alice")
        .WithLastName("Johnson")
        .WithPhone("555-9876")
        .Build();

    public Customer MinimalCustomer => new CustomerBuilder()
        .WithMinimalInfo()
        .Build();

    public Customer CustomerWithLongName => new CustomerBuilder()
        .WithFirstName("VeryLongFirstNameThatMightCauseIssues")
        .WithLastName("VeryLongLastNameThatMightCauseIssues")
        .Build();

    public Customer CustomerWithSpecialChars => new CustomerBuilder()
        .WithFirstName("José")
        .WithLastName("O'Connor-Müller")
        .WithEmail("jose.oconnor@example.com")
        .Build();
}