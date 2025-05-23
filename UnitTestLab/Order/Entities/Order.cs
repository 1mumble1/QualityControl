namespace Order.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsConfirmed { get; set; }

    public void Confirm() => IsConfirmed = true;
}
