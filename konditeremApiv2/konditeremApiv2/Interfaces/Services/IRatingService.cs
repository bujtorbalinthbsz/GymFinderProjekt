using System.Security.Claims;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Interfaces.Services;

public interface IRatingService
{
    public Task<List<RatingResponse>> GetAllAsync();
    public Task<RatingResponse?> GetByIdAsync(int id);
    public Task<RatingResponse?> CreateAsync(CreateRatingRequest request, ClaimsPrincipal currentUser);
    public Task<bool> UpdateAsync(int id, UpdateRatingRequest request, ClaimsPrincipal currentUser);
    public Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser);
}