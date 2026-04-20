using DotNetExamApi.Domain.Services;
using DotNetExamApi.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetExamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;
    private readonly AppDbContext _dbContext;

    public InventoryController(InventoryService inventoryService, AppDbContext dbContext)
    {
        _inventoryService = inventoryService;
        _dbContext = dbContext;
    }

    [HttpGet("{productId}/stock")]
    public async Task<IActionResult> GetStock(int productId, CancellationToken cancellationToken)
    {
        var stock = await _inventoryService.GetStockAsync(productId).ConfigureAwait(false);
        return Ok(new { ProductId = productId, StockQuantity = stock });
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10,
        CancellationToken cancellationToken = default)
    {
        var products = await _dbContext.Products
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new { p.Id, p.Name, p.StockQuantity })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Ok(products);
    }

    [HttpPost("{productId}/update")]
    public async Task<IActionResult> UpdateInventory(int productId, [FromBody] int delta,
        CancellationToken cancellationToken)
    {
        await _inventoryService.ProcessInventoryUpdateAsync(productId, delta);
        return NoContent();
    }

    [HttpPost("{productId}/reserve")]
    public async Task<IActionResult> ReserveStock(int productId, [FromBody] int quantity,
        CancellationToken cancellationToken)
    {
        var reserved = await _inventoryService.CheckAndReserveAsync(productId, quantity);
        if (!reserved)
            return Conflict(new { Message = "Insufficient stock available" });

        return Ok(new { ProductId = productId, Reserved = quantity });
    }

    [HttpDelete("{productId}/reservation")]
    public IActionResult CancelReservation(int productId, [FromQuery] int quantity)
    {
        _inventoryService.CancelReservation(productId, quantity);
        return NoContent();
    }
}
