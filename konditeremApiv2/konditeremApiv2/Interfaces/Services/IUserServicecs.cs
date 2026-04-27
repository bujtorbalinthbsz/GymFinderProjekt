using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using System.Security.Claims;

namespace konditeremApiv2.Interfaces.Services;

public interface IUserService
{
    public Task<List<UserResponse>> GetAllAsync();
    public Task<UserResponse?> GetByIdAsync(int id);
    public Task<UserResponse?> GetCurrentAsync(ClaimsPrincipal currentUser);
    public Task<bool> UpdateAsync(int id, UpdateUserRequest request, ClaimsPrincipal currentUser);
    public Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser);
}