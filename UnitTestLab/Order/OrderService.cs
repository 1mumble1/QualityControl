using Order.Abstractions;

namespace Order;

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly INotificationService _notification;

    public int TotalCreatedOrders { get; private set; }
    public int TotalConfirmedOrders { get; private set; }

    public OrderService(IOrderRepository repository, INotificationService notification)
    {
        _repository = repository;
        _notification = notification;
    }

    public Entities.Order CreateOrder(string email, decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be positive");
        }

        var order = new Entities.Order
        {
            Id = Guid.NewGuid(),
            CustomerEmail = email,
            Amount = amount
        };

        _repository.SaveOrder(order);
        TotalCreatedOrders++;
        return order;
    }

    public void ConfirmOrder(Guid orderId)
    {
        var order = _repository.GetOrderById(orderId) ?? throw new InvalidOperationException("Order not found");
        if (order.IsConfirmed)
        {
            return;
        }

        order.Confirm();
        _repository.SaveOrder(order);
        _notification.NotifyCustomer($"Order {order.Id} confirmed.");
        TotalConfirmedOrders++;
    }

    public void CancelOrder(Guid orderId)
    {
        var order = _repository.GetOrderById(orderId) ?? throw new InvalidOperationException("Order not found");
        _repository.DeleteOrder(orderId);
        _notification.NotifyCustomer($"Order {order.Id} cancelled.");

        if (order.IsConfirmed)
        {
            TotalConfirmedOrders--;
        }
        TotalCreatedOrders--;
    }

    public Entities.Order? GetOrder(Guid orderId)
    {
        return _repository.GetOrderById(orderId);
    }

    public void UpdateOrderAmount(Guid orderId, decimal newAmount)
    {
        if (newAmount <= 0)
        {
            throw new ArgumentException("Amount must be positive");
        }

        var order = _repository.GetOrderById(orderId) ?? throw new InvalidOperationException("Order not found");
        order.Amount = newAmount;
        _repository.SaveOrder(order);
    }
}
