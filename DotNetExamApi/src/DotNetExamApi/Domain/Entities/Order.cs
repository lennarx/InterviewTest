namespace DotNetExamApi.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(CustomerName)) return false;
        if (string.IsNullOrWhiteSpace(CustomerEmail)) return false;
        if (Items == null || !Items.Any()) return false;
        return true;
    }

    public decimal CalculateTotal()
    {
        TotalAmount = Items?.Sum(i => i.UnitPrice * i.Quantity) ?? 0m;
        return TotalAmount;
    }

    public string FormatForReport()
    {
        return $"Order #{Id} | Customer: {CustomerName} | Items: {Items?.Count ?? 0} | " +
               $"Total: {TotalAmount:C} | Status: {Status} | Date: {CreatedAt:yyyy-MM-dd}";
    }

    public string GetStatusDescription()
    {
        switch (Status)
        {
            case OrderStatus.Pending:
                return "Order is pending confirmation";
            case OrderStatus.Processing:
                return "Order is being processed";
            case OrderStatus.Shipped:
                return "Order has been shipped";
            case OrderStatus.Delivered:
                return "Order has been delivered successfully";
            case OrderStatus.Cancelled:
                return "Order was cancelled";
            default:
                return "Unknown status";
        }
    }
}
