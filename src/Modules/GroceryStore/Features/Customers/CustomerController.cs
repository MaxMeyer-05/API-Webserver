using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using SharedKernel.Security.Interfaces;
using GroceryStore.Features.Customers.Interfaces;

namespace GroceryStore.Features.Customers;

/// <summary>
/// Controller for managing customers in the grocery store module.
/// </summary>
[ApiController]
[Tags("Customer")]
[Produces("application/json")]
[Route("api/module/grocery-store/customers")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICurrentUser _currentUser;
    private readonly ITokenService _tokenService;

    public CustomerController(
        ICustomerService customerService,
        ICurrentUser currentUser,
        ITokenService tokenService)
    {
        _customerService = customerService;
        _currentUser = currentUser;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Retrieves a list of all customers.
    /// </summary>
    /// <returns>A list of all customers.</returns>
    /// <response code="200">Returns the list of customers.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        return Ok(await _customerService.GetAllCustomersAsync());
    }

    /// <summary>
    /// Retrieves the currently authenticated customer's details.
    /// </summary>
    /// <returns>The details of the currently authenticated customer.</returns>
    /// <response code="200">Returns the details of the currently authenticated customer.</response>
    /// <response code="404">If the customer is not found.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto?>> GetCurrentCustomer()
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(_currentUser.UserId);
            return Ok(customer);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new customer in the system.
    /// </summary>
    /// <param name="customer">The customer registration details.</param>
    /// <returns>The created CustomerDto object.</returns>
    /// <response code="201">Returns the newly created customer.</response>
    /// <response code="400">If the email or phone number is already in use.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> CreateCustomer([FromBody] CustomerRegistrationDto customer)
    {
        try
        {
            var createdCustomer = await _customerService.CreateCustomerAsync(customer);
            return CreatedAtAction(nameof(CreateCustomer), createdCustomer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Authenticates a customer and returns their details if successful.
    /// </summary>
    /// <param name="customer">The customer login details.</param>
    /// <returns>The authenticated CustomerDto object.</returns>
    /// <response code="200">Returns the authenticated customer details.</response>
    /// <response code="401">If the authentication fails.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerDto?>> LoginCustomer([FromBody] CustomerLoginDto customer)
    {
        try
        {
            var loggedInCustomer = await _customerService.LoginCustomerAsync(customer);
            var token = _tokenService.GenerateToken(
                loggedInCustomer!.CustomerId,
                loggedInCustomer.Email,
                loggedInCustomer.Role);

            return Ok(new CustomerAuthResponseDto(
                token,
                loggedInCustomer.CustomerId,
                loggedInCustomer.Role));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    /// <summary>
    /// Updates the currently authenticated customer's details.
    /// </summary>
    /// <param name="customer">The customer update details.</param>
    /// <returns>No content if the update is successful.</returns>
    /// <response code="204">If the update is successful.</response>
    /// <response code="400">If the email or phone number is already in use.</response>
    /// <response code="404">If the customer is not found.</response>
    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer([FromBody] CustomerUpdateDto customer)
    {
        try
        {
            await _customerService.UpdateCustomerAsync(_currentUser.UserId, customer);
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
    /// Deletes the currently authenticated customer from the system.
    /// </summary>
    /// <param name="password">The password of the customer to confirm deletion.</param>
    /// <response code="204">If the deletion is successful.</response>
    /// <response code="400">If the password is invalid.</response>
    /// <response code="404">If the customer is not found.</response>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer([FromBody] string password)
    {
        try
        {
            await _customerService.DeleteCustomerAsync(_currentUser.UserId, password);
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