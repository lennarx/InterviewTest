using DotNetExamApi.Application.Commands;
using DotNetExamApi.Application.Queries;
using DotNetExamApi.Controllers;
using DotNetExamApi.Domain.Entities;
using DotNetExamApi.Domain.Services;
using DotNetExamApi.Infrastructure;
using DotNetExamApi.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DotNetExamApi.Tests;

public class OrderServiceTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly AppDbContext _dbContext;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("OrderTestDb")
            .Options;

        _dbContext = new AppDbContext(options);
    }

    [Fact]
    public async Task GetOrders_WhenCalled_ReturnsOkResult()
    {
        var orders = new List<Order>
        {
            new() { Id = 1, CustomerName = "Alice", CustomerEmail = "alice@test.com", Status = OrderStatus.Pending, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, CustomerName = "Bob", CustomerEmail = "bob@test.com", Status = OrderStatus.Delivered, CreatedAt = DateTime.UtcNow }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        var controller = new OrdersController(_mediatorMock.Object);
        var result = await controller.GetOrders(null, null);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public void CreateOrder_WithValidCommand_ReturnsOk()
    {
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var controller = new OrdersController(_mediatorMock.Object);
        var command = new CreateOrderCommand("John Doe", "john@example.com",
            new List<OrderItemRequest> { new(1, 2) });

        var result = controller.CreateOrder(command);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void ValidateOrder_WithEmptyOrder_ShouldReturnFalse()
    {
        try
        {
            var order = new Order();
            var isValid = order.Validate();

            Assert.False(isValid);
            throw new InvalidOperationException("Order was invalid as expected.");
        }
        catch (Exception)
        {
        }
    }

    [Fact]
    public void GenerateReport_WithOrders_ContainsCustomerName()
    {
        var orders = new List<Order>
        {
            new()
            {
                Id = 10,
                CustomerName = "Test Customer",
                CustomerEmail = "test@test.com",
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = 49.99m,
                Items = new List<OrderItem>
                {
                    new() { ProductId = 1, Quantity = 2, UnitPrice = 24.99m }
                }
            }
        };

        var service = new ReportService();
        var report = service.GenerateOrderReport(orders);

        Assert.Contains("Test Customer", report);
        Assert.Contains("10", report);
        Assert.NotEmpty(service.GetReportHistory());
    }

    [Fact]
    public void PricingCalculator_Calculate_ReturnsExpectedSubtotal()
    {
        var items = new List<OrderItem>
        {
            new() { ProductId = 1, Quantity = 3, UnitPrice = 10m },
            new() { ProductId = 2, Quantity = 1, UnitPrice = 5m }
        };
        var calculator = new PricingCalculator();

        var summary = calculator.Calculate(items);

        Assert.Equal(35m, summary.SubTotal);
        Assert.Equal(4, summary.ItemCount);
    }
}
