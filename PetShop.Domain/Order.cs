namespace PetShop.Domain;

/// <summary>
/// Represents an order in the pet shop system.
/// Orders progress through three states: Open → Processing → Delivered.
/// Each state has specific rules governing what operations are allowed.
/// </summary>
public class Order
{
    /// <summary>
    /// Gets the unique identifier for the order.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the identifier of the customer who placed this order.
    /// This value is immutable after order creation.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// Gets the current status of the order.
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// Gets the scheduled pickup date for the order.
    /// </summary>
    public DateOnly PickupDate { get; private set; }

    /// <summary>
    /// Gets the actual cost of the order when delivered, if available.
    /// This is set when the order transitions to Delivered status.
    /// </summary>
    public decimal? ActualCost { get; private set; }

    /// <summary>
    /// Gets the customer who placed this order.
    /// </summary>
    public Customer Customer { get; private set; } = null!;

    /// <summary>
    /// Gets the collection of pets in this order.
    /// </summary>
    public ICollection<Pet> Pets { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/> class.
    /// </summary>
    /// <param name="customerId">The identifier of the customer placing the order. Cannot be empty.</param>
    /// <param name="pickupDate">The scheduled pickup date. Must be today or in the future.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when customerId is empty or pickupDate is in the past.</exception>
    public Order(Guid customerId, DateOnly pickupDate)
    {
        ValidateCustomerId(customerId);
        ValidatePickupDate(pickupDate);

        Id = Guid.NewGuid();
        CustomerId = customerId;
        Status = OrderStatus.Open;
        PickupDate = pickupDate;
        ActualCost = null;
        Pets = new List<Pet>();
    }

    /// <summary>
    /// Transitions the order from Open to Processing status.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Thrown when order is not in Open status.</exception>
    /// <exception cref="BusinessRuleViolationException">Thrown when order has no pets.</exception>
    public void TransitionToProcessing()
    {
        if (Status != OrderStatus.Open)
        {
            throw new InvalidOrderStateException(
                $"Cannot transition from {Status} to Processing. Order must be in Open status.");
        }

        if (!Pets.Any())
        {
            throw new BusinessRuleViolationException(
                "Order must have at least one pet before transitioning to Processing.");
        }

        Status = OrderStatus.Processing;
    }

    /// <summary>
    /// Transitions the order from Processing to Delivered status.
    /// Sets the ActualCost to the sum of all pet prices at the time of delivery.
    /// </summary>
    /// <exception cref="InvalidOrderStateException">Thrown when order is not in Processing status.</exception>
    public void TransitionToDelivered()
    {
        if (Status != OrderStatus.Processing)
        {
            throw new InvalidOrderStateException(
                $"Cannot transition from {Status} to Delivered. Order must be in Processing status.");
        }

        Status = OrderStatus.Delivered;
        ActualCost = CalculateEstimatedCost();
    }

    /// <summary>
    /// Adds a pet to the order.
    /// </summary>
    /// <param name="pet">The pet to add. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when pet is null.</exception>
    /// <exception cref="InvalidOrderStateException">Thrown when order is not in Open status.</exception>
    public void AddPet(Pet pet)
    {
        if (pet == null)
        {
            throw new ArgumentNullException(nameof(pet));
        }

        if (Status != OrderStatus.Open)
        {
            throw new InvalidOrderStateException(
                $"Cannot add pets to order in {Status} status. Pets can only be added when order is Open.");
        }

        Pets.Add(pet);
    }

    /// <summary>
    /// Removes a pet from the order.
    /// </summary>
    /// <param name="pet">The pet to remove. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when pet is null.</exception>
    /// <exception cref="InvalidOrderStateException">Thrown when order is not in Open status.</exception>
    public void RemovePet(Pet pet)
    {
        if (pet == null)
        {
            throw new ArgumentNullException(nameof(pet));
        }

        if (Status != OrderStatus.Open)
        {
            throw new InvalidOrderStateException(
                $"Cannot remove pets from order in {Status} status. Pets can only be removed when order is Open.");
        }

        if (!Pets.Remove(pet))
        {
            throw new ArgumentException("Pet is not in this order.", nameof(pet));
        }
    }

    /// <summary>
    /// Updates the pickup date for the order.
    /// </summary>
    /// <param name="pickupDate">The new pickup date. Must be today or in the future.</param>
    /// <exception cref="BusinessRuleViolationException">Thrown when pickupDate is in the past.</exception>
    /// <exception cref="InvalidOrderStateException">Thrown when order is in Delivered status.</exception>
    public void UpdatePickupDate(DateOnly pickupDate)
    {
        if (Status == OrderStatus.Delivered)
        {
            throw new InvalidOrderStateException(
                "Cannot modify pickup date for order in Delivered status. Order is immutable.");
        }

        ValidatePickupDate(pickupDate);
        PickupDate = pickupDate;
    }

    /// <summary>
    /// Calculates the estimated cost of the order based on current pet prices.
    /// </summary>
    /// <returns>The sum of all pet prices in the order.</returns>
    public decimal CalculateEstimatedCost()
    {
        return Pets.Sum(pet => pet.Price);
    }

    private static void ValidateCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new BusinessRuleViolationException("Customer ID is required and cannot be empty.");
        }
    }

    private static void ValidatePickupDate(DateOnly pickupDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (pickupDate < today)
        {
            throw new BusinessRuleViolationException(
                $"Invalid pickup date: must be today or in the future. Provided date: {pickupDate:yyyy-MM-dd}, Today: {today:yyyy-MM-dd}");
        }
    }
}
