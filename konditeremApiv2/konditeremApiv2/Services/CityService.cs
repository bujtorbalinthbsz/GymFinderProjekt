using System.Security.Claims;
using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using konditeremApiv2.Models;
using Microsoft.EntityFrameworkCore;

namespace konditeremApiv2.Services;

public class CityService(AppDbContext context) : ICityService
{
    private static readonly HashSet<string> AllowedRoles = ["admin"];
    
    public async Task<List<CityResponse>> GetAllAsync()
        => await context.Cities
            .Select(c => c.GetResponse())
            .ToListAsync();

    public async Task<CityResponse?> GetByIdAsync(int id)
        => await context.Cities
            .Where(c => c.Id == id)
            .Select(c => c.GetResponse())
            .FirstOrDefaultAsync();

    public async Task<CityResponse?> CreateAsync(CreateCityRequest request, ClaimsPrincipal currentUser)
    {
        if (!AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return null;
        }

        var city = new City
        {
            Name = request.Name,
            ZipCode = request.ZipCode
        };
        
        context.Cities.Add(city);
        await context.SaveChangesAsync();
        
        return city.GetResponse();
    }

    public async Task<bool> UpdateAsync(int id, UpdateCityRequest request, ClaimsPrincipal currentUser)
    {
        var city =  await context.Cities.FindAsync(id);

        if (city == null || city.Id != request.Id || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }
        
        city.Name = request.Name;
        city.ZipCode = request.ZipCode;
        
        context.Cities.Update(city);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser)
    {
        var  city = await context.Cities.FindAsync(id);

        if (city == null || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }
        
        context.Cities.Remove(city);
        await context.SaveChangesAsync();
        
        return true;
    }
    
    private static string GetCurrentUserRole(ClaimsPrincipal currentUser)
    {
        var role = currentUser.FindFirstValue(ClaimTypes.Role);
        
        return string.IsNullOrWhiteSpace(role) ? throw new UnauthorizedAccessException("Missing role claim.") : role;
    }
}