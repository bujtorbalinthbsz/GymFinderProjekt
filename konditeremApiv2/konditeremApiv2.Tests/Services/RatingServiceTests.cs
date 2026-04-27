using System.Security.Claims;
using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.Models;
using konditeremApiv2.Services;
using Microsoft.EntityFrameworkCore;

namespace konditeremApiv2.Tests.Services;

public class RatingServiceTests
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
    public async Task CreateAsync_ReturnsNull_WhenRoleIsNotAllowed()
    {
        await using var context = CreateContext();
        var service = new RatingService(context);

        var result = await service.CreateAsync(new CreateRatingRequest
        {
            Stars = 5,
            Message = "Great",
            UserId = 11,
            GymId = 2
        }, CreatePrincipal(11, "cashier"));

        Assert.Null(result);
        Assert.Empty(context.Ratings);
    }

    [Fact]
    public async Task CreateAsync_CreatesRating_WhenRoleIsUser()
    {
        await using var context = CreateContext();
        var service = new RatingService(context);

        var result = await service.CreateAsync(new CreateRatingRequest
        {
            Stars = 4,
            Message = "Nice",
            UserId = 10,
            GymId = 7
        }, CreatePrincipal(10, "user"));

        Assert.NotNull(result);
        Assert.Equal(4, (await context.Ratings.FirstAsync()).Stars);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenIdMismatch()
    {
        await using var context = CreateContext();
        context.Ratings.Add(new Rating { Id = 3, Stars = 2, Message = "Old", UserId = 1, GymId = 2 });
        await context.SaveChangesAsync();
        var service = new RatingService(context);

        var result = await service.UpdateAsync(3, new UpdateRatingRequest
        {
            Id = 4,
            Stars = 5,
            Message = "Updated"
        }, CreatePrincipal(1, "admin"));

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesRating_WhenRoleIsAdmin()
    {
        await using var context = CreateContext();
        context.Ratings.Add(new Rating { Id = 9, Stars = 3, Message = "Ok", UserId = 1, GymId = 2 });
        await context.SaveChangesAsync();
        var service = new RatingService(context);

        var result = await service.DeleteAsync(9, CreatePrincipal(1, "admin"));

        Assert.True(result);
        Assert.Empty(context.Ratings);
    }
}