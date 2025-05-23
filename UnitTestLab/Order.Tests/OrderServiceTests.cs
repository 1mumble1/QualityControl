using Moq;
using Order.Abstractions;

namespace Order.Tests;

public class OrderServiceTests
{
    [Fact]
    public void Create_Order_With_Valid_Input_Should_Create_Order()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        // Act
        Entities.Order order = service.CreateOrder("test@example.com", 100m);

        // Assert
        Assert.Equal("test@example.com", order.CustomerEmail);
        Assert.Equal(100m, order.Amount);
        mockRepository.Verify(r => r.SaveOrder(order), Times.Once);

        Assert.Equal(1, service.TotalCreatedOrders);
        Assert.Equal(0, service.TotalConfirmedOrders);
    }

    [Fact]
    public void Create_Order_With_Negative_Amount_Throws_Exception()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        // Act
        Func<Entities.Order> act = () => service.CreateOrder("a@a.com", -10m);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Confirm_Valid_Order_Confirms_And_Notifies()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        Entities.Order order = service.CreateOrder("a@a.ru", 50m);
        mockRepository.Setup(r => r.GetOrderById(order.Id)).Returns(order);

        // Act
        service.ConfirmOrder(order.Id);

        // Assert
        Assert.True(order.IsConfirmed);
        mockRepository.Verify(r => r.SaveOrder(order), Times.Exactly(2));
        mockService.Verify(n => n.NotifyCustomer(It.IsAny<string>()), Times.Once);
        Assert.Equal(1, service.TotalConfirmedOrders);
        Assert.Equal(1, service.TotalCreatedOrders);
    }

    [Fact]
    public void Confirm_Not_Found_Order_Throws_Exception()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        mockRepository.Setup(r => r.GetOrderById(It.IsAny<Guid>())).Returns((Entities.Order?)null);

        // Act
        Action act = () => service.ConfirmOrder(Guid.NewGuid());

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Confirm_Already_Confirmed_Order_Skips()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        Entities.Order order = service.CreateOrder("a@a.ru", 50m);
        order.Confirm();
        mockRepository.Setup(r => r.GetOrderById(order.Id)).Returns(order);

        // Act
        service.ConfirmOrder(order.Id);

        // Assert
        Assert.True(order.IsConfirmed);
        Assert.Equal(1, service.TotalCreatedOrders);
        Assert.Equal(0, service.TotalConfirmedOrders);
    }

    [Fact]
    public void Cancel_Not_Confirmed_Order_Should_Delete_And_Notify()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        Entities.Order order = service.CreateOrder("a@a.ru", 100m);
        mockRepository.Setup(r => r.GetOrderById(order.Id)).Returns(order);

        // Act
        service.CancelOrder(order.Id);

        // Assert
        mockRepository.Verify(r => r.DeleteOrder(order.Id), Times.Once);
        mockService.Verify(n => n.NotifyCustomer(It.IsAny<string>()), Times.Once);
        Assert.Equal(0, service.TotalConfirmedOrders);
        Assert.Equal(0, service.TotalCreatedOrders);
    }

    [Fact]
    public void Cancel_Confirmed_Order_Should_Delete_And_Notify()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        Entities.Order order = service.CreateOrder("a@a.ru", 100m);
        mockRepository.Setup(r => r.GetOrderById(order.Id)).Returns(order);
        service.ConfirmOrder(order.Id);

        // Act
        service.CancelOrder(order.Id);

        // Assert
        mockRepository.Verify(r => r.DeleteOrder(order.Id), Times.Once);
        mockService.Verify(n => n.NotifyCustomer(It.IsAny<string>()), Times.Exactly(2));
        Assert.Equal(0, service.TotalConfirmedOrders);
        Assert.Equal(0, service.TotalCreatedOrders);
    }

    [Fact]
    public void Update_Order_Amount_On_Valid_Amount_Should_Update()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        Entities.Order order = service.CreateOrder("a@a.ru", 100m);
        mockRepository.Setup(r => r.GetOrderById(order.Id)).Returns(order);

        // Act
        service.UpdateOrderAmount(order.Id, 200m);

        // Assert
        Assert.Equal(200m, order.Amount);
        mockRepository.Verify(r => r.SaveOrder(order), Times.Exactly(2));
    }

    [Fact]
    public void Update_Order_Amount_Not_Found_Order_Throws_Exception()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        // Act
        Action act = () => service.UpdateOrderAmount(Guid.NewGuid(), 200m);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
    }

    [Fact]
    public void Update_Order_Amount_On_Invalid_Amount_Throws_Exception()
    {
        // Arrange
        Mock<IOrderRepository> mockRepository = new();
        Mock<INotificationService> mockService = new();
        OrderService service = new(mockRepository.Object, mockService.Object);

        // Act
        Action act = () => service.UpdateOrderAmount(Guid.NewGuid(), 0);

        Assert.Throws<ArgumentException>(act);
    }
}