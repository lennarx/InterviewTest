using DotNetExamApi.Domain.Entities;

namespace DotNetExamApi.Shared;

public struct CartSummary
{
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }

    public void ApplyPromotion(decimal discountRate)
    {
        DiscountAmount = SubTotal * (discountRate / 100m);
        TotalAmount = SubTotal - DiscountAmount + TaxAmount + ShippingCost;
    }
}

public class PricingCalculator
{
    private const decimal TaxRate = 0.21m;
    private const decimal StandardShipping = 5.99m;
    private const decimal FreeShippingThreshold = 50m;

    public CartSummary Calculate(List<OrderItem> items)
    {
        var subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
        var itemCount = items.Sum(i => i.Quantity);
        var shipping = subtotal >= FreeShippingThreshold ? 0m : StandardShipping;
        var tax = subtotal * TaxRate;

        return new CartSummary
        {
            SubTotal = subtotal,
            TaxAmount = tax,
            DiscountAmount = 0m,
            ShippingCost = shipping,
            TotalAmount = subtotal + tax + shipping,
            ItemCount = itemCount
        };
    }

    public CartSummary CalculateWithDiscount(List<OrderItem> items, decimal discountRate)
    {
        var summary = Calculate(items);
        summary.ApplyPromotion(discountRate);
        return summary;
    }

    public void LogSummary(CartSummary summary)
    {
        object obj = summary;
        Console.WriteLine($"[Pricing] Cart summary: {obj}");
        Console.WriteLine($"[Pricing] Subtotal={summary.SubTotal:C}, Tax={summary.TaxAmount:C}, " +
                          $"Shipping={summary.ShippingCost:C}, Total={summary.TotalAmount:C}");
    }

    public string FormatSummary(CartSummary summary)
    {
        return $"Items: {summary.ItemCount} | Subtotal: {summary.SubTotal:C} | " +
               $"Tax: {summary.TaxAmount:C} | Shipping: {summary.ShippingCost:C} | " +
               $"Total: {summary.TotalAmount:C}";
    }
}
