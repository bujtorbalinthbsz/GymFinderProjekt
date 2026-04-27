using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.Models;
using konditeremApiv2.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace konditeremApiv2.Tests.Services;

public class CityServiceTests
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
        var service = new CityService(context);

        var result = await service.CreateAsync(new CreateCityRequest { Name = "Bp", ZipCode = 1111 }, CreatePrincipal("user"));

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesCity_WhenAdminAndIdsMatch()
    {
        await using var context = CreateContext();
        context.Cities.Add(new City { Id = 1, Name = "Old", ZipCode = 1111 });
        await context.SaveChangesAsync();
        var service = new CityService(context);

        var result = await service.UpdateAsync(1, new UpdateCityRequest { Id = 1, Name = "New", ZipCode = 2222 }, CreatePrincipal("admin"));

        Assert.True(result);
        Assert.Equal("New", (await context.Cities.FindAsync(1))!.Name);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenMissing()
    {
        await using var context = CreateContext();
        var service = new CityService(context);

        var result = await service.DeleteAsync(42, CreatePrincipal("admin"));

        Assert.False(result);
    }
}
