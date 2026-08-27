using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using SharedKernel.Security.Interfaces;
using GroceryStore.Features.Suppliers.Interfaces;

namespace GroceryStore.Features.Suppliers;

/// <summary>
/// Controller for managing suppliers in the grocery store module.
/// </summary>
[ApiController]
[Tags("Supplier")]
[Produces("application/json")]
[Route("api/module/grocery-store/suppliers")]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ICurrentUser _currentUser;
    private readonly ITokenService _tokenService;

    public SupplierController(
        ISupplierService supplierService,
        ICurrentUser currentUser,
        ITokenService tokenService)
    {
        _supplierService = supplierService;
        _currentUser = currentUser;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Retrieves a list of all suppliers.
    /// </summary>
    /// <returns>A list of SupplierDto objects.</returns>
    /// <response code="200">Returns the list of suppliers.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<SupplierDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
    {
        return Ok(await _supplierService.GetAllSuppliersAsync());
    }

    /// <summary>
    /// Retrieves a specific supplier by their unique identifier.
    /// </summary>
    /// <param name="supplierId">The unique identifier of the supplier.</param>
    /// <returns>A SupplierDto object representing the supplier.</returns>
    /// <response code="200">Returns the supplier details.</response>
    /// <response code="404">If the supplier is not found.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SupplierDto>> GetSupplierById()
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByIdAsync(_currentUser.UserId);
            return Ok(supplier);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new supplier in the system.
    /// </summary>
    /// <param name="supplier">The supplier registration details.</param>
    /// <returns>The created SupplierDto object.</returns>
    /// <response code="201">Returns the newly created supplier.</response>
    /// <response code="400">If the supplier registration details are invalid.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupplier([FromBody] SupplierRegistrationDto supplier)
    {
        try
        {
            var createdSupplier = await _supplierService.CreateSupplierAsync(supplier);
            return CreatedAtAction(nameof(GetSupplierById), createdSupplier);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Authenticates a supplier and returns their details if successful.
    /// </summary>
    /// <param name="supplier">The supplier login credentials.</param>
    /// <returns>The authenticated SupplierDto object.</returns>
    /// <response code="200">Returns the authenticated supplier details.</response>
    /// <response code="401">If the authentication fails.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginSupplier([FromBody] SupplierLoginDto supplier)
    {
        try
        {
            var loggedInSupplier = await _supplierService.LoginSupplierAsync(supplier);
            var token = _tokenService.GenerateToken(
                loggedInSupplier!.SupplierId, 
                loggedInSupplier.Email, 
                loggedInSupplier.Role);

            return Ok(new AuthResponseDto(
                token,
                loggedInSupplier.SupplierId,
                loggedInSupplier.Role));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ex.Message);
        }
    }

    /// <summary>
    /// Updates the details of an existing supplier.
    /// </summary>
    /// <param name="supplier">The updated supplier details.</param>
    /// <response code="204">Indicates that the supplier was successfully updated.</response>
    /// <response code="400">If the update details are invalid.</response>
    /// <response code="404">If the supplier is not found.</response>
    [Authorize]
    [HttpPatch("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSupplier([FromBody] SupplierUpdateDto supplier)
    {
        try
        {
            await _supplierService.UpdateSupplierAsync(_currentUser.UserId, supplier);
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
    /// Deletes a supplier from the system.
    /// </summary>
    /// <param name="password">The password of the supplier to confirm deletion.</param>
    /// <response code="204">Indicates that the supplier was successfully deleted.</response>
    /// <response code="400">If the password is invalid.</response>
    /// <response code="404">If the supplier is not found.</response>
    [Authorize]
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSupplier([FromBody] SupplierActionRequest supplierActionRequest)
    {
        try
        {
            await _supplierService.DeleteSupplierAsync(_currentUser.UserId, supplierActionRequest.Password);
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