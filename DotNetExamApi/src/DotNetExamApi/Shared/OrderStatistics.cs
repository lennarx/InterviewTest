using DotNetExamApi.Domain.Entities;
using DotNetExamApi.Domain.Services;
using DotNetExamApi.Infrastructure;

namespace DotNetExamApi.Shared;

public class OrderSummaryResult
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int PendingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public string ReportText { get; set; } = string.Empty;
}

public class OrderStatistics
{
    private readonly AppDbContext _dbContext;

    public OrderStatistics(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Order>> GetRecentOrdersAsync(int count = 10)
    {
        return _dbContext.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToList();
    }

    public static OrderSummaryResult Calculate(List<Order> orders)
    {
        var reportService = new ReportService();
        var calculator = new PricingCalculator();

        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var avgValue = orders.Count > 0 ? totalRevenue / orders.Count : 0m;

        var allItems = orders.SelectMany(o => o.Items).ToList();
        var pricingSummary = allItems.Any()
            ? calculator.Calculate(allItems)
            : new CartSummary();

        var reportText = reportService.GenerateOrderReport(orders);

        return new OrderSummaryResult
        {
            TotalOrders = orders.Count,
            TotalRevenue = totalRevenue,
            AverageOrderValue = avgValue,
            PendingOrders = orders.Count(o => o.Status == OrderStatus.Pending),
            CompletedOrders = orders.Count(o => o.Status == OrderStatus.Delivered),
            ReportText = reportText
        };
    }
}
