using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Model;
using System.Security.Claims;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly OrderDbContext _db;

    public OrderController(OrderDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value;
        var orders = await _db.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.Name)?.Value;

        var order = new Order
        {
            UserId = userId!,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            Price = request.Price,
            Status = "Pending"
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Order created successfully", orderId = order.Id, order });
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();

        order.Status = status;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Status updated", order });
    }
}

public record CreateOrderRequest(string ProductName, int Quantity, decimal Price);