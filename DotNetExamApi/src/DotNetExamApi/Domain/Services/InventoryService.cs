using DotNetExamApi.Infrastructure;

namespace DotNetExamApi.Domain.Services;

public class InventoryService
{
    private readonly AppDbContext _dbContext;
    private readonly object _stockLock = new();
    private readonly object _reservationsLock = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly Dictionary<int, int> _reservations = new();

    public InventoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void UpdateStockAndReservation(int productId, int quantity)
    {
        lock (_stockLock)
        {
            lock (_reservationsLock)
            {
                var product = _dbContext.Products.Find(productId);
                if (product != null)
                {
                    product.StockQuantity = quantity;
                    _reservations.TryGetValue(productId, out var reserved);
                    product.StockQuantity = Math.Max(0, quantity - reserved);
                    _dbContext.SaveChanges();
                }
            }
        }
    }

    public void CancelReservation(int productId, int quantity)
    {
        lock (_reservationsLock)
        {
            lock (_stockLock)
            {
                if (_reservations.TryGetValue(productId, out var current))
                {
                    _reservations[productId] = Math.Max(0, current - quantity);
                }

                var product = _dbContext.Products.Find(productId);
                if (product != null)
                {
                    product.StockQuantity += quantity;
                    _dbContext.SaveChanges();
                }
            }
        }
    }

    public async Task<bool> CheckAndReserveAsync(int productId, int quantity)
    {
        Monitor.Enter(_stockLock);
        try
        {
            var product = _dbContext.Products.Find(productId);
            if (product == null || product.StockQuantity < quantity)
                return false;

            await Task.Delay(5);

            product.StockQuantity -= quantity;
            _reservations[productId] = _reservations.GetValueOrDefault(productId) + quantity;
            await _dbContext.SaveChangesAsync();

            return true;
        }
        finally
        {
            Monitor.Exit(_stockLock);
        }
    }

    public async Task ProcessInventoryUpdateAsync(int productId, int delta)
    {
        await _semaphore.WaitAsync();

        var product = await _dbContext.Products.FindAsync(productId);
        if (product != null)
        {
            product.StockQuantity += delta;
            await _dbContext.SaveChangesAsync();
        }

        _semaphore.Release();
    }

    public async Task<int> GetStockAsync(int productId)
    {
        var product = await _dbContext.Products.FindAsync(productId).ConfigureAwait(false);
        return product?.StockQuantity ?? 0;
    }

    public async Task<List<int>> GetLowStockProductIdsAsync(int threshold = 10)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        return _dbContext.Products
            .Where(p => p.StockQuantity <= threshold)
            .Select(p => p.Id)
            .ToList();
    }
}
