using konditeremApiv2.Data;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using konditeremApiv2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace konditeremApiv2.Services;

public class UserService(AppDbContext context) : IUserService
{
    private readonly PasswordHasher<object> _passwordHasher = new();
    private static readonly HashSet<string> AllowedRoles = ["admin", "user"];

    public async Task<List<UserResponse>> GetAllAsync() => await context.Users
        .Select(c => c.GetResponse())
        .ToListAsync();

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        return await context.Users.Where(c => c.Id == id)
            .Select(c => c.GetResponse())
            .FirstOrDefaultAsync();
        ;
    }

    public async Task<UserResponse?> GetCurrentAsync(ClaimsPrincipal currentUser)
    {
        var currentUserId = GetCurrentUserId(currentUser);

        return await context.Users
            .Where(c => c.Id == currentUserId)
            .Select(c => c.GetResponse())
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateUserRequest request, ClaimsPrincipal currentUser)
    {
        var user = await context.Users.FindAsync(id);

        if (user is null || id != request.Id || !AllowedRoles.Contains(GetCurrentUserRole(currentUser)))
        {
            return false;
        }

        var callerId = GetCurrentUserId(currentUser);
        var callerRole = GetCurrentUserRole(currentUser);
        var isAdmin = callerRole == "admin";
        var isSelf = callerId == id;

        if (!isAdmin && !isSelf)
        {
            throw new UnauthorizedAccessException("Only admins can update other users.");
        }

        user.Name = request.Name;
        user.Email = request.Email;

        if (isAdmin)
        {
            user.Role = request.Role;
        }
        else if (!string.Equals(request.Role, user.Role, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Users cannot change their own role.");
        }
        
        if (request.Password is not null)
        {
            user.Password = _passwordHasher.HashPassword(null, request.Password);
        }

        context.Users.Update(user);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser)
    {
        var user = await context.Users.FindAsync(id);

        if (user is null)
        {
            return false;
        }

        var callerId = GetCurrentUserId(currentUser);
        var callerRole = GetCurrentUserRole(currentUser);
        var isAdmin = callerRole == "admin";
        var isSelf = callerId == id;

        if (!isAdmin && !isSelf)
        {
            throw new UnauthorizedAccessException("Only admins can delete other users.");
        }

        context.Users.Remove(user);
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

        return string.IsNullOrWhiteSpace(role) ? throw new UnauthorizedAccessException("Missing role claim.") : role;
    }
}