using Microsoft.EntityFrameworkCore;

using GroceryStore.Database.DbContexts;
using GroceryStore.Features.Users.Interfaces;

namespace GroceryStore.Features.Users;

/// <summary>
/// Service class for managing user data in the database.
/// </summary>
public class UserService : IUserService
{
    private readonly GroceryStoreDbContext _dbContext;
    private readonly IUserMapper _userMapper;
    private readonly ILogger<UserService> _logger;

    public UserService(
        GroceryStoreDbContext dbContext, 
        IUserMapper userMapper,
        ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _userMapper = userMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<UserDto> CreateUserAsync(UserRegistrationDto user)
    {
        var userEntity = _userMapper.ToUserEntity(user);

        if (await IsEmailInUseAsync(userEntity.Email))
            throw new InvalidOperationException("Email is already in use.");

        if (userEntity.PhoneNumber is not null 
            && await IsPhoneNumberInUseAsync(userEntity.PhoneNumber))
            throw new InvalidOperationException("Phone number is already in use.");

        await _dbContext.Users.AddAsync(userEntity);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Created new user with Id {UserId}", userEntity.Id);
        return _userMapper.ToUserDto(userEntity);
    }

    /// <inheritdoc />
    public async Task DeleteUserAsync(Guid userId, string password)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"User with Id {userId} not found");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new InvalidOperationException("Invalid password.");

        _userMapper.AnonymizeUserEntity(user);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Anonymized user with Id {UserId}", userId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _dbContext.Users.ToListAsync();
        return users.Select(u => _userMapper.ToUserDto(u));
    }

    /// <inheritdoc />
    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"User with Id {userId} not found");

        return user is null ? null : _userMapper.ToUserDto(user);
    }

    /// <inheritdoc />
    public async Task<bool> IsEmailInUseAsync(string email, Guid? excludedUserId = null)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.Email == email && (excludedUserId == null || u.Id != excludedUserId));
    }

    /// <inheritdoc />
    public async Task<bool> IsPhoneNumberInUseAsync(string phoneNumber, Guid? excludedUserId = null)
    {
        return await _dbContext.Users
            .AnyAsync(u => u.PhoneNumber == phoneNumber && (excludedUserId == null || u.Id != excludedUserId));
    }

    /// <inheritdoc />
    public async Task<UserDto?> LoginUserAsync(UserLoginDto user)
    {
        var userEntity = await _dbContext.Users
            .Where(u => u.Email == user.Email)
            .FirstOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(user.Password, userEntity.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        return userEntity is null ? null : _userMapper.ToUserDto(userEntity);
    }

    /// <inheritdoc />
    public async Task UpdateUserAsync(Guid userId, UserUpdateDto user)
    {
        var userEntity = await _dbContext.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException($"User with Id {userId} not found");

        if (user.Email is not null && await IsEmailInUseAsync(user.Email, userId))
            throw new InvalidOperationException("Email is already in use.");

        if (user.PhoneNumber is not null && await IsPhoneNumberInUseAsync(user.PhoneNumber, userId))
            throw new InvalidOperationException("Phone number is already in use.");

        _userMapper.UpdateUserEntity(userEntity, user);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated user with Id {UserId}", userId);
    }

    /// <summary>
    /// Hashes the provided password using BCrypt.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The hashed password.</returns>
    private static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);
}