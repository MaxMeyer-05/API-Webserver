using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using SharedKernel.Security.Interfaces;
using GroceryStore.Features.Orders.Interfaces;

namespace GroceryStore.Features.Orders;

/// <summary>
/// Controller for managing orders in the grocery store module.
/// </summary>
[Authorize]
[ApiController]
[Tags("Order")]
[Produces("application/json")]
[Route("api/module/grocery-store/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ICurrentUser _currentUser;

    public OrderController(
        IOrderService orderService, 
        ICurrentUser currentUser)
    {
        _orderService = orderService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Retrieves all orders for the current customer.
    /// </summary>
    /// <returns>Returns a collection of <see cref="OrderDto"/> representing the customer's orders.</returns>
    /// <response code="200">Returns a collection of <see cref="OrderDto"/> representing the customer's orders.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllOrders()
    {
        return Ok(await _orderService.GetAllOrdersAsync(_currentUser.UserId));
    }

    /// <summary>
    /// Retrieves a specific order by its order number and customer identifier.
    /// </summary>
    /// <param name="orderNum">The order number of the order to retrieve.</param>
    /// <returns>Returns the <see cref="OrderDto"/> representing the requested order.</returns>
    /// <response code="200">Returns the <see cref="OrderDto"/> representing the requested order.</response>
    /// <response code="404">If the order with the specified order number and customer identifier does not exist.</response>
    [HttpGet("{orderNum}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrderById([FromRoute] int orderNum)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderNum, _currentUser.UserId);
            return Ok(order);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new order for a customer.
    /// </summary>
    /// <param name="order">The <see cref="OrderCreateDto"/> containing the details of the order to be created.</param>
    /// <returns>Returns the created <see cref="OrderDto"/> representing the newly created order.</returns>
    /// <response code="201">Returns the created <see cref="OrderDto"/> representing the newly created order.</response>
    /// <response code="400">If the request body is invalid or if the order cannot be created due to business logic constraints.</response>
    [HttpPost("create")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderCreateDto order)
    {
        try
        {
            var createdOrder = await _orderService.CreateOrderAsync(order);
            return CreatedAtAction(nameof(GetOrderById), new { orderNum = createdOrder.OrderId }, createdOrder);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing order for a customer.
    /// </summary>
    /// <param name="orderNum">The order number of the order to update.</param>
    /// <param name="order">The <see cref="OrderUpdateDto"/> containing the updated details of the order.</param>
    /// <returns>Returns a 204 No Content response if the update is successful.</returns>
    /// <response code="204">If the order is successfully updated.</response>
    /// <response code="400">If the request body is invalid or if the order cannot be updated due to business logic constraints.</response>
    /// <response code="404">If the specified order or customer does not exist.</response>
    /// <response code="403">If the customer is not authorized to update the specified order.</response>
    [HttpPatch("{orderNum}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateOrder(
        [FromRoute] int orderNum,
        [FromBody] OrderUpdateDto order)
    {
        try
        {
            await _orderService.UpdateOrderAsync(orderNum, _currentUser.UserId, order);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}