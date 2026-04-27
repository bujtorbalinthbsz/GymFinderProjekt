using System.Security.Claims;
using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using konditeremApiv2.Models;
using Microsoft.EntityFrameworkCore;

namespace konditeremApiv2.Services;

public class PurchaseService(AppDbContext context) : IPurchaseService
{
    private static readonly HashSet<string> AllowedRoles = ["admin", "user"];

    public async Task<List<PurchaseResponse>> GetAllAsync()
        => await context.Purchases
            .Include(p => p.Product)
            .Include(p => p.Gym)
            .Select(p => p.GetResponse())
            .ToListAsync();

    public async Task<List<PurchaseResponse>> GetByUserIdAsync(int userId)
        => await context.Purchases
            .Include(p => p.Product)
            .Include(p => p.Gym)
            .Where(p => p.UserId == userId)
            .Select(p => p.GetResponse())
            .ToListAsync();

    public async Task<PurchaseResponse?> GetByIdAsync(Guid id)
        => await context.Purchases
            .Include(p => p.Product)
            .Include(p => p.Gym)
            .Where(p => p.Id == id)
            .Select(p => p.GetResponse())
            .FirstOrDefaultAsync();

    public async Task<PurchaseResponse?> CreateAsync(CreatePurchaseRequest request, ClaimsPrincipal currentUser)
    {
        if (!AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return null;
        }

        var purchase = new Purchase
        {
            IsCash = request.IsCash,
            IsCreditCard = request.IsCreditCard,
            Amount = request.Amount,
            ExpirationDate = request.ExpirationDate,
            ProductId = request.ProductId,
            UserId = request.UserId,
            GymId = request.GymId,
            CashierUserId = GetCurrentUserId(currentUser)
        };

        context.Purchases.Add(purchase);
        await context.SaveChangesAsync();

        return purchase.GetResponse();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdatePurchaseRequest request, ClaimsPrincipal currentUser)
    {
        var purchase = await context.Purchases.FindAsync(id);

        if (purchase == null || purchase.Id != request.Id || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }

        purchase.Amount = request.Amount;
        purchase.ExpirationDate = request.ExpirationDate;
        purchase.ProductId = request.ProductId;
        purchase.UserId = request.UserId;
        purchase.CashierUserId = GetCurrentUserId(currentUser);

        context.Purchases.Update(purchase);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, ClaimsPrincipal currentUser)
    {
        var purchase = await context.Purchases.FindAsync(id);

        if (purchase == null || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }

        context.Purchases.Remove(purchase);
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