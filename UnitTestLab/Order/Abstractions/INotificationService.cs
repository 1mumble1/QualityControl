namespace Order.Abstractions;

public interface INotificationService
{
    void NotifyCustomer(string message);
}
