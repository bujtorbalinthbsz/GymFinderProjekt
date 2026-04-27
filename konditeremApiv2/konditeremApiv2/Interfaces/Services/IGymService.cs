using System.Security.Claims;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Interfaces.Services;

public interface IGymService
{
    public Task<List<GymResponse>> GetAllAsync();
    public Task<GymResponse?> GetByIdAsync(int id);
    public Task<GymResponse?> CreateAsync(CreateGymRequest request, ClaimsPrincipal currentUser);
    public Task<bool> UpdateAsync(int id, UpdateGymRequest request, ClaimsPrincipal currentUser);
    public Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser);
}