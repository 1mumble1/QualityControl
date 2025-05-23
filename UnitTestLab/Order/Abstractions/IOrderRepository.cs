namespace Order.Abstractions;

public interface IOrderRepository
{
    Entities.Order? GetOrderById(Guid id);
    void SaveOrder(Entities.Order order);
    void DeleteOrder(Guid id);
}
