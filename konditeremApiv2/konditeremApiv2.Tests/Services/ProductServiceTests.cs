using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.Models;
using konditeremApiv2.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace konditeremApiv2.Tests.Services;

public class ProductServiceTests
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
    public async Task CreateAsync_CreatesProductWithGyms_WhenAdmin()
    {
        await using var context = CreateContext();
        var service = new ProductService(context);

        var result = await service.CreateAsync(
            new CreateProductRequest { Designation = "Monthly", GymIds = [1, 2], IsRental = false, IsTicket = true },
            CreatePrincipal("admin"));

        Assert.NotNull(result);
        Assert.Equal(2, (await context.Products.Include(p => p.Gyms).FirstAsync()).Gyms!.Count);
    }

    [Fact]
    public async Task CreateAsync_CreatesProductWithoutGyms_WhenAdmin()
    {
        await using var context = CreateContext();
        var service = new ProductService(context);

        var result = await service.CreateAsync(
            new CreateProductRequest { Designation = "Monthly", IsRental = false, IsTicket = true },
            CreatePrincipal("admin"));

        Assert.NotNull(result);
        Assert.Equal(0, (await context.Products.Include(p => p.Gyms).FirstAsync()).Gyms!.Count);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenRoleIsNotAdmin()
    {
        await using var context = CreateContext();
        context.Products.Add(new Product { Id = 1, Designation = "Old", IsActive = true });
        await context.SaveChangesAsync();
        var service = new ProductService(context);

        var result = await service.UpdateAsync(1, new UpdateProductRequest
        {
            Id = 1,
            Designation = "New",
            GymIds = [1],
            IsActive = true
        }, CreatePrincipal("user"));

        Assert.False(result);
    }

    [Fact]
    public async Task ActivateDeactivateAsync_TogglesState_WhenAdmin()
    {
        await using var context = CreateContext();
        context.Products.Add(new Product { Id = 9, Designation = "P", IsActive = false });
        await context.SaveChangesAsync();
        var service = new ProductService(context);

        var activated = await service.ActivateAsync(9, CreatePrincipal("admin"));
        var deactivated = await service.DeactivateAsync(9, CreatePrincipal("admin"));

        Assert.True(activated);
        Assert.True(deactivated);
        Assert.False((await context.Products.FindAsync(9))!.IsActive);
    }
}
