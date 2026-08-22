using Microsoft.AspNetCore.Mvc;
using GroceryStore.Models;

namespace GroceryStore.Controllers;

[ApiController]
[Tags("Order")]
[Produces("application/json")]
[Route("api/module/grocery-store/orders")]
public class OrderController : ControllerBase
{
    [HttpGet]
    public IActionResult GetOrders()
    {
        return Ok();
    }

    [HttpGet("{orderId}")]
    public IActionResult GetOrderById([FromRoute] int orderId)
    {
        return Ok();
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] OrderCreateDto order)
    {
        return Created();
    }

    [HttpPatch("{orderId}")]
    public IActionResult UpdateOrder([FromRoute] int orderId, [FromBody] OrderUpdateDto order)
    {
        return NoContent();
    }

    [HttpDelete("{orderId}")]
    public IActionResult DeleteOrder([FromRoute] int orderId)
    {
        return NoContent();
    }
}