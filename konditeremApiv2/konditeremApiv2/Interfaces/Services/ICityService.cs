using System.Security.Claims;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Interfaces.Services;

public interface ICityService
{
    public Task<List<CityResponse>> GetAllAsync();
    public Task<CityResponse?> GetByIdAsync(int id);
    public Task<CityResponse?> CreateAsync(CreateCityRequest request, ClaimsPrincipal currentUser);
    public Task<bool> UpdateAsync(int id, UpdateCityRequest request, ClaimsPrincipal currentUser);
    public Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser);
}