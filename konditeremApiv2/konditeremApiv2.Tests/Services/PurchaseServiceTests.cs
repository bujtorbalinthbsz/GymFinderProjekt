using System.Security.Claims;
using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.Models;
using konditeremApiv2.Services;
using Microsoft.EntityFrameworkCore;

namespace konditeremApiv2.Tests.Services;

public class PurchaseServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static ClaimsPrincipal CreatePrincipal(int id, string role)
        => new(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, role)],
            "test"));

    [Fact]
    public async Task CreateAsync_ReturnsNull_WhenRoleIsNotAdmin()
    {
        await using var context = CreateContext();
        var service = new PurchaseService(context);

        var result = await service.CreateAsync(new CreatePurchaseRequest
        {
            IsCash = true,
            IsCreditCard = false,
            Amount = 1500,
            ProductId = 1,
            UserId = 2,
            GymId = 1
        }, CreatePrincipal(99, "user"));

        Assert.Null(result);
        Assert.Empty(context.Purchases);
    }

    [Fact]
    public async Task CreateAsync_CreatesPurchase_WhenRoleIsAdmin()
    {
        await using var context = CreateContext();
        var service = new PurchaseService(context);

        var result = await service.CreateAsync(new CreatePurchaseRequest
        {
            IsCash = false,
            IsCreditCard = true,
            Amount = 3200,
            ProductId = 3,
            UserId = 4,
            GymId = 1
        }, CreatePrincipal(11, "admin"));

        Assert.NotNull(result);
        Assert.Equal(11, (await context.Purchases.FirstAsync()).CashierUserId);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenIdMismatch()
    {
        await using var context = CreateContext();
        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            Amount = 1000,
            ProductId = 1,
            UserId = 2,
            CashierUserId = 9
        };
        context.Purchases.Add(purchase);
        await context.SaveChangesAsync();
        var service = new PurchaseService(context);

        var result = await service.UpdateAsync(purchase.Id, new UpdatePurchaseRequest
        {
            Id = Guid.NewGuid(),
            Amount = 2000,
            ProductId = 5,
            UserId = 6
        }, CreatePrincipal(1, "admin"));

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesPurchase_WhenRoleIsAdmin()
    {
        await using var context = CreateContext();
        var purchase = new Purchase
        {
            Id = Guid.NewGuid(),
            Amount = 1000,
            ProductId = 1,
            UserId = 2,
            CashierUserId = 9
        };
        context.Purchases.Add(purchase);
        await context.SaveChangesAsync();
        var service = new PurchaseService(context);

        var result = await service.DeleteAsync(purchase.Id, CreatePrincipal(1, "admin"));

        Assert.True(result);
        Assert.Empty(context.Purchases);
    }
}