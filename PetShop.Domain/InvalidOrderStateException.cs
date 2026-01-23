namespace PetShop.Domain;

/// <summary>
/// Exception thrown when an order state transition or operation violates state rules.
/// </summary>
public class InvalidOrderStateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOrderStateException"/> class.
    /// </summary>
    public InvalidOrderStateException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOrderStateException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public InvalidOrderStateException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidOrderStateException"/> class with a specified error message
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public InvalidOrderStateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
