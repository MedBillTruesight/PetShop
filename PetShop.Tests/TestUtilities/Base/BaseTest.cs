using FluentAssertions;

/// <summary>
/// Base test class providing common functionality for all tests.
/// Includes common setup, teardown, and utility methods.
/// </summary>
public abstract class BaseTest : IDisposable
{
    protected readonly TestCustomerFixture CustomerFixture;
    protected readonly TestOrderFixture OrderFixture;
    protected readonly TestPetFixture PetFixture;

    protected BaseTest()
    {
        CustomerFixture = new TestCustomerFixture();
        OrderFixture = new TestOrderFixture();
        PetFixture = new TestPetFixture();
    }

    /// <summary>
    /// Override this method to perform custom setup before each test.
    /// </summary>
    protected virtual void Setup()
    {
    }

    /// <summary>
    /// Override this method to perform custom teardown after each test.
    /// </summary>
    protected virtual void TearDown()
    {
    }

    /// <summary>
    /// Asserts that an action throws a specific exception type.
    /// </summary>
    protected static async Task AssertThrowsAsync<TException>(Func<Task> action, string because = "")
        where TException : Exception
    {
        var exception = await action.Should().ThrowAsync<TException>(because);
    }

    /// <summary>
    /// Asserts that an action throws a specific exception type with synchronous code.
    /// </summary>
    protected static void AssertThrows<TException>(Action action, string because = "")
        where TException : Exception
    {
        action.Should().Throw<TException>(because);
    }

    /// <summary>
    /// Generates a new GUID for test data.
    /// </summary>
    protected static Guid NewGuid() => Guid.NewGuid();

    /// <summary>
    /// Gets today's date for testing.
    /// </summary>
    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Gets tomorrow's date for testing.
    /// </summary>
    protected static DateOnly Tomorrow => Today.AddDays(1);

    /// <summary>
    /// Gets yesterday's date for testing.
    /// </summary>
    protected static DateOnly Yesterday => Today.AddDays(-1);

    public void Dispose()
    {
        TearDown();
        GC.SuppressFinalize(this);
    }
}
