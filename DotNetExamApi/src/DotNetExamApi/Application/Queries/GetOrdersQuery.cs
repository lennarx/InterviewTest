using DotNetExamApi.Domain.Entities;
using DotNetExamApi.Infrastructure;
using MediatR;

namespace DotNetExamApi.Application.Queries;

public record GetOrdersQuery(OrderStatus? StatusFilter = null, DateTime? FromDate = null) : IRequest<List<Order>>;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<Order>>
{
    private readonly AppDbContext _dbContext;

    public GetOrdersQueryHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Order>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Order> orders = _dbContext.GetAllOrders();

        if (request.StatusFilter.HasValue)
        {
            orders = orders.Where(o => o.Status == request.StatusFilter.Value);
        }

        if (request.FromDate.HasValue)
        {
            orders = orders.Where(o => o.CreatedAt >= request.FromDate.Value);
        }

        return await Task.FromResult(orders.ToList());
    }
}
