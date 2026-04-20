using DotNetExamApi.Domain.Entities;
using DotNetExamApi.Domain.Services;
using DotNetExamApi.Infrastructure;
using DotNetExamApi.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = false;
    options.ValidateOnBuild = false;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "DotNetExamApi", Version = "v1" });
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("DotNetExamDb"));

builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddSingleton<OrderStatistics>();
builder.Services.AddSingleton<FileLoggerService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var path = config["FileLogger:FilePath"] ?? "logs/app.log";
    return new FileLoggerService(path);
});
builder.Services.AddSingleton<ServiceBusPublisher>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DotNetExamApi v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

_ = SeedDatabaseAsync(app);

app.Run();

static async Task SeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.EnsureCreatedAsync();

    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product { Id = 1, Name = "Widget A", Description = "Standard widget", Price = 9.99m, StockQuantity = 100 },
            new Product { Id = 2, Name = "Widget B", Description = "Premium widget", Price = 19.99m, StockQuantity = 50 },
            new Product { Id = 3, Name = "Gadget X", Description = "Basic gadget", Price = 34.99m, StockQuantity = 25 },
            new Product { Id = 4, Name = "Gadget Y", Description = "Advanced gadget", Price = 79.99m, StockQuantity = 10 }
        );

        await context.SaveChangesAsync();
    }
}
