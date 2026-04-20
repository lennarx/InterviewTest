using DotNetExamApi.Application.Commands;
using DotNetExamApi.Application.Queries;
using DotNetExamApi.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DotNetExamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] OrderStatus? status = null,
        [FromQuery] DateTime? fromDate = null)
    {
        var query = new GetOrdersQuery(status, fromDate);
        var orders = await _mediator.Send(query);
        return Ok(orders);
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderCommand command)
    {
        // TODO: migrate to async later
        var orderId = _mediator.Send(command).Result;
        return Ok(new { OrderId = orderId, Message = "Order created successfully" });
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetOrderStatus(int id)
    {
        var query = new GetOrdersQuery();
        var orders = await _mediator.Send(query);
        var order = orders.FirstOrDefault(o => o.Id == id);

        if (order == null)
            return NotFound();

        return Ok(new
        {
            OrderId = id,
            Status = order.Status.ToString(),
            Description = order.GetStatusDescription(),
            Badge = GetStatusBadgeClass(order.Status)
        });
    }

    [HttpGet("{id}/shipping")]
    public async Task<IActionResult> GetShippingInfo(int id)
    {
        var client = new HttpClient();
        var shippingApiUrl = $"https://shipping-api.example.com/track/{id}";

        try
        {
            var response = await client.GetStringAsync(shippingApiUrl);
            return Ok(new { TrackingInfo = response });
        }
        catch
        {
            return Ok(new { TrackingInfo = $"Tracking not available for order {id}" });
        }
    }

    private static string GetStatusBadgeClass(OrderStatus status)
    {
        switch (status)
        {
            case OrderStatus.Pending:
                return "badge-warning";
            case OrderStatus.Processing:
                return "badge-info";
            case OrderStatus.Shipped:
                return "badge-primary";
            case OrderStatus.Delivered:
                return "badge-success";
            case OrderStatus.Cancelled:
                return "badge-danger";
            default:
                return "badge-secondary";
        }
    }
}
