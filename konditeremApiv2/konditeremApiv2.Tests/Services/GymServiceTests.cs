using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.Models;
using konditeremApiv2.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json.Nodes;

namespace konditeremApiv2.Tests.Services;

public class GymServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static ClaimsPrincipal CreatePrincipal(string role)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test"));

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenRoleIsNotAdmin()
    {
        await using var context = CreateContext();
        var service = new GymService(context);

        var result = await service.CreateAsync(
            new CreateGymRequest { Name = "Gym", OpenAt = JsonNode.Parse("{}")!.AsObject() },
            CreatePrincipal("user"));

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenIdMismatch()
    {
        await using var context = CreateContext();
        context.Gyms.Add(new Gym { Id = 1, Name = "A", OpenAt = "{}" });
        await context.SaveChangesAsync();
        var service = new GymService(context);

        var result = await service.UpdateAsync(1,
            new UpdateGymRequest { Id = 2, Name = "B", OpenAt = JsonNode.Parse("{}")!.AsObject() },
            CreatePrincipal("admin"));

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_DeletesGym_WhenAdmin()
    {
        await using var context = CreateContext();
        context.Gyms.Add(new Gym { Id = 5, Name = "DeleteMe", OpenAt = "{}" });
        await context.SaveChangesAsync();
        var service = new GymService(context);

        var result = await service.DeleteAsync(5, CreatePrincipal("admin"));

        Assert.True(result);
        Assert.Null(await context.Gyms.FindAsync(5));
    }
}
