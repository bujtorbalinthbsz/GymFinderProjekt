using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace konditeremApiv2.Tests.Services;

public class UserServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static ClaimsPrincipal CreatePrincipal(int id, string role, string? nameIdentifier = null, string? name = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role)
        };

        if (nameIdentifier is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifier));
        }

        if (name is not null)
        {
            claims.Add(new Claim(ClaimTypes.Name, name));
        }

        if (nameIdentifier is null && name is null)
        {
            claims.Add(new Claim("sub", id.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static async Task SeedUsersAsync(AppDbContext context)
    {
        var hasher = new PasswordHasher<object>();
        context.Users.AddRange(
            new() { Id = 1, Name = "Admin", Email = "admin@test.com", Password = hasher.HashPassword(null, "p1"), Role = "admin" },
            new() { Id = 2, Name = "User", Email = "user@test.com", Password = hasher.HashPassword(null, "p2"), Role = "user" },
            new() { Id = 3, Name = "Other", Email = "other@test.com", Password = hasher.HashPassword(null, "p3"), Role = "user" }
        );
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        var result = await service.GetAllAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        await using var context = CreateContext();
        var service = new UserService(context);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentAsync_UsesNameIdentifierClaim()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        var result = await service.GetCurrentAsync(CreatePrincipal(0, "user", nameIdentifier: "2"));

        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
    }

    [Fact]
    public async Task GetCurrentAsync_ThrowsUnauthorized_WhenUserIdClaimInvalid()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.GetCurrentAsync(CreatePrincipal(0, "user", nameIdentifier: "not-number")));
    }

    [Fact]
    public async Task UpdateAsync_AllowsAdminToUpdateOtherUserRole()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        var updated = await service.UpdateAsync(3, new UpdateUserRequest
        {
            Id = 3,
            Name = "Changed",
            Email = "changed@test.com",
            Role = "admin"
        }, CreatePrincipal(1, "admin", nameIdentifier: "1"));

        Assert.True(updated);
        var user = await context.Users.FindAsync(3);
        Assert.Equal("admin", user!.Role);
    }

    [Fact]
    public async Task UpdateAsync_AllowsUserToUpdateSelfWithoutRoleChange()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        var updated = await service.UpdateAsync(2, new UpdateUserRequest
        {
            Id = 2,
            Name = "Updated User",
            Email = "updated@test.com",
            Role = "user"
        }, CreatePrincipal(2, "user", nameIdentifier: "2"));

        Assert.True(updated);
        var user = await context.Users.FindAsync(2);
        Assert.Equal("Updated User", user!.Name);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsUnauthorized_WhenUserUpdatesAnotherUser()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.UpdateAsync(3, new UpdateUserRequest
            {
                Id = 3,
                Name = "x",
                Email = "x@test.com",
                Role = "user"
            }, CreatePrincipal(2, "user", nameIdentifier: "2")));
    }

    [Fact]
    public async Task UpdateAsync_ThrowsUnauthorized_WhenUserChangesOwnRole()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.UpdateAsync(2, new UpdateUserRequest
            {
                Id = 2,
                Name = "user",
                Email = "user@test.com",
                Role = "admin"
            }, CreatePrincipal(2, "user", nameIdentifier: "2")));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_OnIdMismatch_AndAllowsAdminToSetRequestedRole()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        var mismatchResult = await service.UpdateAsync(2, new UpdateUserRequest
        {
            Id = 3,
            Name = "user",
            Email = "user@test.com",
            Role = "user"
        }, CreatePrincipal(1, "admin", nameIdentifier: "1"));

        var invalidRoleResult = await service.UpdateAsync(2, new UpdateUserRequest
        {
            Id = 2,
            Name = "user",
            Email = "user@test.com",
            Role = "super-admin"
        }, CreatePrincipal(1, "admin", nameIdentifier: "1"));

        Assert.False(mismatchResult);
        Assert.True(invalidRoleResult);
    }

    [Fact]
    public async Task DeleteAsync_AllowsAdminToDeleteAnyUser()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        var deleted = await service.DeleteAsync(3, CreatePrincipal(1, "admin", nameIdentifier: "1"));

        Assert.True(deleted);
        Assert.Null(await context.Users.FindAsync(3));
    }

    [Fact]
    public async Task DeleteAsync_AllowsUserToDeleteSelf()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        var deleted = await service.DeleteAsync(2, CreatePrincipal(2, "user", nameIdentifier: "2"));

        Assert.True(deleted);
        Assert.Null(await context.Users.FindAsync(2));
    }

    [Fact]
    public async Task DeleteAsync_ThrowsUnauthorized_WhenUserDeletesAnotherUser()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.DeleteAsync(3, CreatePrincipal(2, "user", nameIdentifier: "2")));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenUserNotFound()
    {
        await using var context = CreateContext();
        await SeedUsersAsync(context);
        var service = new UserService(context);

        var deleted = await service.DeleteAsync(999, CreatePrincipal(1, "admin", nameIdentifier: "1"));

        Assert.False(deleted);
    }
}
