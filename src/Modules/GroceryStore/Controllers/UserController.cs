using Microsoft.AspNetCore.Mvc;
using GroceryStore.Models;

namespace GroceryStore.Controllers;

[ApiController]
[Tags("User")]
[Produces("application/json")]
[Route("api/module/grocery-store/users")]
public class UserController : ControllerBase
{
    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok();
    }

    [HttpGet("{userId}")]
    public IActionResult GetUserById([FromRoute] Guid userId)
    {
        return Ok();
    }

    [HttpPost("register")]
    public IActionResult CreateUser([FromBody] UserRegistrationDto user)
    {
        return Created();
    }

    [HttpPost("login")]
    public IActionResult LoginUser([FromBody] UserLoginDto user)
    {
        return Ok();
    }

    [HttpPatch("{userId}")]
    public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] UserUpdateDto user)
    {
        return NoContent();
    }

    [HttpDelete("{userId}")]
    public IActionResult DeleteUser([FromRoute] Guid userId)
    {
        return NoContent();
    }
}