using DotNetExamApi.Domain.Entities;
using DotNetExamApi.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DotNetExamApi.MinimalApi;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/products", async (AppDbContext db, CancellationToken ct) =>
        {
            return await db.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        });

        app.MapGet("/products/{id:int}", async (int id, AppDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            return Results.Ok(product);
        });

        app.MapPost("/products", async (Product product, AppDbContext db) =>
        {
            db.Products.Add(product);
            await db.SaveChangesAsync();
            return Results.Created($"/products/{product.Id}", product);
        });

        app.MapPut("/products/{id:int}", async (int id, Product updated, AppDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product == null) return Results.NotFound();

            product.Name = updated.Name;
            product.Description = updated.Description;
            product.Price = updated.Price;
            product.StockQuantity = updated.StockQuantity;

            await db.SaveChangesAsync();
            return Results.Ok(product);
        });

        app.MapDelete("/products/{id:int}", async (int id, AppDbContext db) =>
        {
            var product = await db.Products.FindAsync(id);
            if (product == null) return Results.NotFound();

            db.Products.Remove(product);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        app.MapGet("/products/search", async (string? name, decimal? maxPrice, AppDbContext db,
            CancellationToken ct) =>
        {
            var query = db.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.Contains(name));

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.OrderBy(p => p.Price).ToListAsync(ct).ConfigureAwait(false);
        });
    }
}
