using DotNetExamApi.Domain.Entities;
using DotNetExamApi.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DotNetExamApi.Application.Commands;

public record OrderItemRequest(int ProductId, int Quantity);

public record CreateOrderCommand(
    string CustomerName,
    string CustomerEmail,
    List<OrderItemRequest> Items) : IRequest<int>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly AppDbContext _dbContext;
    private readonly ServiceBusPublisher _publisher;

    public CreateOrderCommandHandler(AppDbContext dbContext, ServiceBusPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var products = await _dbContext.Products
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var order = new Order
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItem>()
        };

        foreach (var item in request.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId);
            if (product == null) continue;

            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        if (!order.Validate())
            throw new InvalidOperationException("Order validation failed: missing required fields or items.");

        order.CalculateTotal();

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        await _publisher.PublishOrderCreatedAsync(order.Id);

        return order.Id;
    }
}
