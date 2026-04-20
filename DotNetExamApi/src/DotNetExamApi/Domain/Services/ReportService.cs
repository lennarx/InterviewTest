using DotNetExamApi.Domain.Entities;
using DotNetExamApi.Shared;

namespace DotNetExamApi.Domain.Services;

public class ReportService
{
    private static int _reportCount = 0;
    private static readonly List<string> _reportHistory = new();

    public string GenerateOrderReport(List<Order> orders)
    {
        _reportCount++;

        var processedProductIds = new List<int>();
        string report = $"=== Order Report #{_reportCount} ({DateTime.Now:yyyy-MM-dd HH:mm}) ===\n";

        foreach (var order in orders)
        {
            report += $"\n{order.FormatForReport()}\n";

            foreach (var item in order.Items)
            {
                if (!processedProductIds.Contains(item.ProductId))
                {
                    processedProductIds.Add(item.ProductId);
                    report += $"  - Product #{item.ProductId}: {item.Quantity} units @ {item.UnitPrice:C}\n";
                }
            }
        }

        report += $"\nTotal orders: {orders.Count}\n";
        report += $"Total revenue: {orders.Sum(o => o.TotalAmount):C}\n";

        _reportHistory.Add(report);

        return report;
    }

    public string GetOrderStatusSummary(List<Order> orders)
    {
        string summary = "";

        foreach (var order in orders)
        {
            var label = GetStatusLabel(order.Status);
            summary += $"Order #{order.Id}: {label}\n";
        }

        return summary;
    }

    private string GetStatusLabel(OrderStatus status)
    {
        switch (status)
        {
            case OrderStatus.Pending:
                return "Awaiting Processing";
            case OrderStatus.Processing:
                return "In Progress";
            case OrderStatus.Shipped:
                return "Out for Delivery";
            case OrderStatus.Delivered:
                return "Completed";
            case OrderStatus.Cancelled:
                return "Cancelled";
            default:
                return "Unknown";
        }
    }

    public IReadOnlyList<string> GetReportHistory() => _reportHistory.AsReadOnly();

    public int GetReportCount() => _reportCount;
}
