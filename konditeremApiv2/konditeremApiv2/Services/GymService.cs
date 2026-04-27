using System.Security.Claims;
using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using konditeremApiv2.Models;
using Microsoft.EntityFrameworkCore;

namespace konditeremApiv2.Services;

public class GymService(AppDbContext context) : IGymService
{
    private static readonly HashSet<string> AllowedRoles = ["admin"];

    public async Task<List<GymResponse>> GetAllAsync()
        => await context.Gyms
            .Include(g => g.Products)
            .Select(g => g.GetResponse())
            .ToListAsync();

    public async Task<GymResponse?> GetByIdAsync(int id)
        => await context.Gyms
            .Include(g => g.Products)
            .Where(g => g.Id == id)
            .Select(g => g.GetResponse())
            .FirstOrDefaultAsync();

    public async Task<GymResponse?> CreateAsync(CreateGymRequest request, ClaimsPrincipal currentUser)
    {
        if (!AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return null;
        }

        var gym = new Gym
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            CityId = request.CityId,
            OpenAt = request.OpenAt.ToJsonString(),
            Products = request.ProductIds?.Select(productId => new GymHasProduct
            {
                ProductId = productId
            }).ToList() ?? new List<GymHasProduct>()
        };

        context.Gyms.Add(gym);
        await context.SaveChangesAsync();

        return gym.GetResponse();
    }

    public async Task<bool> UpdateAsync(int id, UpdateGymRequest request, ClaimsPrincipal currentUser)
    {
        var gym = await context.Gyms
            .Include(g => g.Products)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (gym is null || gym.Id != request.Id || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }

        gym.Name = request.Name;
        gym.Phone = request.Phone;
        gym.Email = request.Email;
        gym.CityId = request.CityId;
        gym.OpenAt = request.OpenAt.ToJsonString();

        gym.Products ??= new List<GymHasProduct>();

        if (request.ProductIds != null)
        {
            var existingProductIds = gym.Products
                .Select(p => p.ProductId)
                .ToList();

            var toRemove = gym.Products
                .Where(p => !request.ProductIds.Contains(p.ProductId))
                .ToList();

            if (toRemove.Count > 0)
            {
                context.GymHasProducts.RemoveRange(toRemove);
            }

            var toAdd = request.ProductIds
                .Where(productId => !existingProductIds.Contains(productId))
                .Select(productId => new GymHasProduct
                {
                    GymId = gym.Id,
                    ProductId = productId
                })
                .ToList();

            if (toAdd.Count > 0)
            {
                foreach (var item in toAdd)
                {
                    gym.Products.Add(item);
                }
            }
        }

        context.Gyms.Update(gym);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser)
    {
        var gym = await context.Gyms.FindAsync(id);

        if (gym is null || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }

        context.Gyms.Remove(gym);
        await context.SaveChangesAsync();

        return true;
    }

    private static string GetCurrentUserRole(ClaimsPrincipal currentUser)
    {
        var role = currentUser.FindFirstValue(ClaimTypes.Role);

        return string.IsNullOrWhiteSpace(role)
            ? throw new UnauthorizedAccessException("Missing role claim.")
            : role;
    }
}