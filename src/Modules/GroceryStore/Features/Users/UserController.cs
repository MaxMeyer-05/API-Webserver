using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using SharedKernel.Security.Interfaces;
using GroceryStore.Features.Users.Interfaces;

namespace GroceryStore.Features.Users;

/// <summary>
/// Controller for managing users in the grocery store module.
/// </summary>
[ApiController]
[Tags("User")]
[Produces("application/json")]
[Route("api/module/grocery-store/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUser _currentUser;
    private readonly ITokenService _tokenService;

    public UserController(
        IUserService userService,
        ICurrentUser currentUser,
        ITokenService tokenService)
    {
        _userService = userService;
        _currentUser = currentUser;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Retrieves a list of all users.
    /// </summary>
    /// <returns>A list of all users.</returns>
    /// <response code="200">Returns the list of users.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        return Ok(await _userService.GetAllUsersAsync());
    }

    /// <summary>
    /// Retrieves the currently authenticated user's details.
    /// </summary>
    /// <returns>The details of the currently authenticated user.</returns>
    /// <response code="200">Returns the details of the currently authenticated user.</response>
    /// <response code="404">If the user is not found.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto?>> GetCurrentUser()
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(_currentUser.UserId);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new user in the system.
    /// </summary>
    /// <param name="user">The user registration details.</param>
    /// <returns>The created UserDto object.</returns>
    /// <response code="201">Returns the newly created user.</response>
    /// <response code="400">If the email or phone number is already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserRegistrationDto user)
    {
        try
        {
            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(CreateUser), createdUser);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Authenticates a user and returns their details if successful.
    /// </summary>
    /// <param name="user">The user login details.</param>
    /// <returns>The authenticated UserDto object.</returns>
    /// <response code="200">Returns the authenticated user details.</response>
    /// <response code="401">If the authentication fails.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto?>> LoginUser([FromBody] UserLoginDto user)
    {
        try
        {
            var loggedInUser = await _userService.LoginUserAsync(user);
            var token = _tokenService.GenerateToken(
                loggedInUser!.UserId, 
                loggedInUser.Email, 
                loggedInUser.Role);

            return Ok(new AuthResponseDto(
                token,
                loggedInUser.UserId,
                loggedInUser.Role));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    /// <summary>
    /// Updates the currently authenticated user's details.
    /// </summary>
    /// <param name="user">The user update details.</param>
    /// <returns>No content if the update is successful.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="400">If the email or phone number is already in use.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser([FromBody] UserUpdateDto user)
    {
        try
        {
            await _userService.UpdateUserAsync(_currentUser.UserId, user);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Deletes the currently authenticated user from the system.
    /// </summary>
    /// <param name="password">The password of the user to confirm deletion.</param>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="400">If the password is invalid.</response>
    /// <response code="404">If the user is not found.</response>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser([FromBody] string password)
    {
        try
        {
            await _userService.DeleteUserAsync(_currentUser.UserId, password);
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
    }
}