using Microsoft.EntityFrameworkCore;

using GroceryStore.Features.Customers.Interfaces;
using GroceryStore.Database.DbContexts;
namespace GroceryStore.Features.Customers;

/// <summary>
/// Service class for managing customer data in the database.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly ICustomerMapper _customerMapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        GroceryStoreDbContext dbContext,
        ICustomerMapper customerMapper,
        ILogger<CustomerService> logger)
    {
        _dbContext = dbContext;
        _customerMapper = customerMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<CustomerDto> CreateCustomerAsync(CustomerRegistrationDto customer)
    {
        var customerEntity = _customerMapper.ToCustomerEntity(customer);

        if (await IsEmailInUseAsync(customerEntity.Email))
            throw new InvalidOperationException("Email is already in use.");

        if (customerEntity.PhoneNumber is not null
            && await IsPhoneNumberInUseAsync(customerEntity.PhoneNumber))
            throw new InvalidOperationException("Phone number is already in use.");

        await _dbContext.Customers.AddAsync(customerEntity);
        await _dbContext.SaveChangesAsync();
        await _dbContext.Entry(customerEntity)
            .Reference(u => u.ZipCodeNavigation)
            .LoadAsync();
        _logger.LogInformation("Created new customer with Id {CustomerId}", customerEntity.Id);
        return _customerMapper.ToCustomerDto(customerEntity);
    }

    /// <inheritdoc />
    public async Task DeleteCustomerAsync(Guid customerId, string password)
    {
        var customer = await _dbContext.Customers
            .Where(u => u.Id == customerId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Customer with Id {customerId} not found");

        if (!BCrypt.Net.BCrypt.Verify(password, customer.PasswordHash))
            throw new InvalidOperationException("Invalid password.");

        _customerMapper.AnonymizeCustomerEntity(customer);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Anonymized customer with Id {CustomerId}", customerId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _dbContext.Customers
            .Include(u => u.ZipCodeNavigation)
            .ToListAsync();
        return customers.Select(customer => _customerMapper.ToCustomerDto(customer));
    }

    /// <inheritdoc />
    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid customerId)
    {
        var customer = await _dbContext.Customers
            .Include(u => u.ZipCodeNavigation)
            .Where(u => u.Id == customerId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Customer with Id {customerId} not found");

        return customer is null ? null : _customerMapper.ToCustomerDto(customer);
    }

    /// <inheritdoc />
    public async Task<bool> IsEmailInUseAsync(string email, Guid? excludedCustomerId = null)
    {
        return await _dbContext.Customers
            .AnyAsync(customer => customer.Email == email && (excludedCustomerId == null || customer.Id != excludedCustomerId));
    }

    /// <inheritdoc />
    public async Task<bool> IsPhoneNumberInUseAsync(string phoneNumber, Guid? excludedCustomerId = null)
    {
        return await _dbContext.Customers
            .AnyAsync(customer => customer.PhoneNumber == phoneNumber && (excludedCustomerId == null || customer.Id != excludedCustomerId));
    }

    /// <inheritdoc />
    public async Task<CustomerDto?> LoginCustomerAsync(CustomerLoginDto customer)
    {
        var customerEntity = await _dbContext.Customers
            .Include(u => u.ZipCodeNavigation)
            .Where(u => u.Email == customer.Email)
            .FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(customer.Password, customerEntity.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return customerEntity is null ? null : _customerMapper.ToCustomerDto(customerEntity);
    }

    /// <inheritdoc />
    public async Task UpdateCustomerAsync(Guid customerId, CustomerUpdateDto customer)
    {
        var customerEntity = await _dbContext.Customers
            .Where(u => u.Id == customerId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"Customer with Id {customerId} not found");

        if (customer.Email is not null && await IsEmailInUseAsync(customer.Email, customerId))
            throw new InvalidOperationException("Email is already in use.");

        if (customer.PhoneNumber is not null && await IsPhoneNumberInUseAsync(customer.PhoneNumber, customerId))
            throw new InvalidOperationException("Phone number is already in use.");

        _customerMapper.UpdateCustomerEntity(customerEntity, customer);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated customer with Id {CustomerId}", customerId);
    }

    /// <summary>
    /// Hashes the provided password using BCrypt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password.</returns>
    private static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);
}