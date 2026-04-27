using System.Security.Claims;
using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using konditeremApiv2.Models;
using Microsoft.EntityFrameworkCore;

namespace konditeremApiv2.Services;

public class RatingService(AppDbContext context) : IRatingService
{
    private static readonly HashSet<string> AllowedRoles = ["admin", "user"];

    public async Task<List<RatingResponse>> GetAllAsync()
        => await context.Ratings
            .Include(r => r.User)
            .Include(r => r.Gym)
            .Select(r => r.GetResponse())
            .ToListAsync();

    public async Task<RatingResponse?> GetByIdAsync(int id)
        => await context.Ratings
            .Include(r => r.User)
            .Include(r => r.Gym)
            .Where(r => r.Id == id)
            .Select(r => r.GetResponse())
            .FirstOrDefaultAsync();

    public async Task<RatingResponse?> CreateAsync(CreateRatingRequest request, ClaimsPrincipal currentUser)
    {
        if (!AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return null;
        }

        var rating = new Rating
        {
            Stars = request.Stars,
            Message = request.Message,
            UserId = GetCurrentUserId(currentUser),
            GymId = request.GymId
        };

        context.Ratings.Add(rating);
        await context.SaveChangesAsync();

        await context.Entry(rating).Reference(r => r.User).LoadAsync();
        await context.Entry(rating).Reference(r => r.Gym).LoadAsync();

        return rating.GetResponse();
    }

    public async Task<bool> UpdateAsync(int id, UpdateRatingRequest request, ClaimsPrincipal currentUser)
    {
        var rating = await context.Ratings.FindAsync(id);

        if (rating == null || rating.Id != request.Id || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }

        rating.Stars = request.Stars;
        rating.Message = request.Message;

        context.Ratings.Update(rating);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser)
    {
        var rating = await context.Ratings.FindAsync(id);

        if (rating == null || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }

        context.Ratings.Remove(rating);
        await context.SaveChangesAsync();

        return true;
    }

    private static int GetCurrentUserId(ClaimsPrincipal currentUser)
    {
        var userIdValue = currentUser.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? currentUser.FindFirstValue(ClaimTypes.Name)
                          ?? currentUser.FindFirstValue("sub");

        if (!int.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token subject.");
        }

        return userId;
    }

    private static string GetCurrentUserRole(ClaimsPrincipal currentUser)
    {
        var role = currentUser.FindFirstValue(ClaimTypes.Role);

        return string.IsNullOrWhiteSpace(role)
            ? throw new UnauthorizedAccessException("Missing role claim.")
            : role;
    }
}