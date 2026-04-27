using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.Models;
using konditeremApiv2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace konditeremApiv2.Tests.Services;

public class AuthServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static IConfiguration CreateConfiguration()
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "very-long-test-secret-key-1234567890",
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        await using var context = CreateContext();
        var hasher = new PasswordHasher<object>();
        context.Users.Add(new User
        {
            Name = "Admin",
            Email = "admin@test.com",
            Password = hasher.HashPassword(null, "password123"),
            Role = "admin"
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateConfiguration());
        var result = await service.LoginAsync(new LoginRequest { Email = "admin@test.com", Password = "password123" });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.Token));
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenEmailDoesNotExist()
    {
        await using var context = CreateContext();
        var service = new AuthService(context, CreateConfiguration());

        var result = await service.LoginAsync(new LoginRequest { Email = "missing@test.com", Password = "password123" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordIsInvalid()
    {
        await using var context = CreateContext();
        var hasher = new PasswordHasher<object>();
        context.Users.Add(new User
        {
            Name = "User",
            Email = "user@test.com",
            Password = hasher.HashPassword(null, "valid-password"),
            Role = "user"
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateConfiguration());
        var result = await service.LoginAsync(new LoginRequest { Email = "user@test.com", Password = "wrong-password" });

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WhenEmailIsUnique()
    {
        await using var context = CreateContext();
        var service = new AuthService(context, CreateConfiguration());

        var result = await service.RegisterAsync(new RegisterRequest
        {
            Name = "New User",
            Email = "new@test.com",
            Password = "password123"
        });

        Assert.NotNull(result);
        Assert.Equal("user", result!.Role);

        var saved = await context.Users.SingleAsync(u => u.Email == "new@test.com");
        Assert.NotEqual("password123", saved.Password);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsNull_WhenEmailAlreadyExists()
    {
        await using var context = CreateContext();
        context.Users.Add(new User
        {
            Name = "Existing",
            Email = "existing@test.com",
            Password = "hashed",
            Role = "user"
        });
        await context.SaveChangesAsync();

        var service = new AuthService(context, CreateConfiguration());
        var result = await service.RegisterAsync(new RegisterRequest
        {
            Name = "Another",
            Email = "existing@test.com",
            Password = "password123"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task LogoutAsync_CompletesSuccessfully()
    {
        await using var context = CreateContext();
        var service = new AuthService(context, CreateConfiguration());

        await service.LogoutAsync();
    }
}
