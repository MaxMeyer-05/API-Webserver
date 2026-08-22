using Microsoft.AspNetCore.Mvc;
using GroceryStore.Models;

namespace GroceryStore.Controllers;

[ApiController]
[Tags("Supplier")]
[Produces("application/json")]
[Route("api/module/grocery-store/suppliers")]
public class SupplierController : ControllerBase
{
    [HttpGet]
    public IActionResult GetSuppliers()
    {
        return Ok();
    }

    [HttpGet("{supplierId}")]
    public IActionResult GetSupplierById([FromRoute] Guid supplierId)
    {
        return Ok();
    }

    [HttpPost("register")]
    public IActionResult CreateSupplier([FromBody] SupplierRegistrationDto supplier)
    {
        return Created();
    }

    [HttpPost("login")]
    public IActionResult LoginSupplier([FromBody] SupplierLoginDto supplier)
    {
        return Ok();
    }

    [HttpPatch("{supplierId}")]
    public IActionResult UpdateSupplier([FromRoute] Guid supplierId, [FromBody] SupplierUpdateDto supplier)
    {
        return NoContent();
    }

    [HttpDelete("{supplierId}")]
    public IActionResult DeleteSupplier([FromRoute] Guid supplierId)
    {
        return NoContent();
    }
}