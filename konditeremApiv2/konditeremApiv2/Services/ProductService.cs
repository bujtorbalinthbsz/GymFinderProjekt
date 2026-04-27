using System.Security.Claims;
using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using konditeremApiv2.Models;
using Microsoft.EntityFrameworkCore;

namespace konditeremApiv2.Services;

public class ProductService(AppDbContext context) : IProductService
{
    private static readonly HashSet<string> AllowedRoles = ["admin"];

    public async Task<List<ProductResponse>> GetAllAsync()
        => await context.Products
            .Select(c => c.GetResponse())
            .ToListAsync();

    public async Task<ProductResponse?> GetByIdAsync(int id)
        => await context.Products
            .Where(c => c.Id == id)
            .Select(c => c.GetResponse())
            .FirstOrDefaultAsync();

    public async Task<ProductResponse?> CreateAsync(CreateProductRequest request, ClaimsPrincipal currentUser)
    {
        if (!AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return null;
        }

        var product = new Product
        {
            Designation = request.Designation,
            IsActive = true,
            IsRental = request.IsRental,
            IsTicket = request.IsTicket,
            Price = request.Price,
            Gyms = request.GymIds?.Select(c => new GymHasProduct {
                GymId = c
            }).ToList()
        };
        
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product.GetResponse();
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductRequest request, ClaimsPrincipal currentUser)
    {
        var product = await context.Products
            .Include(c => c.Gyms)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (product == null || product.Id != request.Id || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }
        
        product.Designation = request.Designation;
        product.IsActive = request.IsActive;
        product.IsRental = request.IsRental;
        product.IsTicket = request.IsTicket;
        product.Price = request.Price;
        
        var existingGymIds =  product.Gyms
            ?.Select(c => c.GymId)
            .ToList();

        if (request.GymIds != null)
        {
            var toRemove = product.Gyms
                ?.Where(c => request.GymIds.Contains(c.GymId))
                .ToList();
            
            if (toRemove != null)
            {
                context.GymHasProducts.RemoveRange(toRemove);
            }
        }

        var toAdd = request.GymIds?
            .Select(c => new GymHasProduct
            {
                GymId = c,
                ProductId = product.Id
            });

        if (existingGymIds != null)
        {
            toAdd = request.GymIds?
                .Where(c => !existingGymIds.Contains(c))
                .Select(c => new GymHasProduct
                {
                    GymId = c,
                    ProductId = product.Id
                });            
        }

        product.Gyms ??= new List<GymHasProduct>();

        foreach (var gymId in toAdd)
        {
            product.Gyms.Add(gymId);
        }
        
        context.Products.Update(product);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser)
    {
        var product = await context.Products
            .Include(c => c.Gyms)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (product == null || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }
        
        var toRemove = product.Gyms
            ?.ToList();
        
        if (toRemove != null)
        {
            context.GymHasProducts.RemoveRange(toRemove);
        }
        
        context.Products.Remove(product);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ActivateAsync(int id, ClaimsPrincipal currentUser)
    {
        var product = await context.Products
            .Include(c => c.Gyms)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (product == null || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }
        
        product.IsActive = true;
        
        context.Products.Update(product);
        await context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeactivateAsync(int id, ClaimsPrincipal currentUser)
    {
        var product = await context.Products
            .Include(c => c.Gyms)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (product == null || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }
        
        product.IsActive = false;
        
        context.Products.Update(product);
        await context.SaveChangesAsync();
        
        return true;
    }

    private static string GetCurrentUserRole(ClaimsPrincipal currentUser)
    {
        var role = currentUser.FindFirstValue(ClaimTypes.Role);

        return string.IsNullOrWhiteSpace(role) ? throw new UnauthorizedAccessException("Missing role claim.") : role;
    }
}