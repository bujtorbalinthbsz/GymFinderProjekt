using System.Security.Claims;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Interfaces.Services;

public interface IProductService
{
    public Task<List<ProductResponse>> GetAllAsync();
    public Task<ProductResponse?> GetByIdAsync(int id);
    public Task<ProductResponse?> CreateAsync(CreateProductRequest request, ClaimsPrincipal currentUser);
    public Task<bool> UpdateAsync(int id, UpdateProductRequest request, ClaimsPrincipal currentUser);
    public Task<bool> DeleteAsync(int id, ClaimsPrincipal currentUser);
    public Task<bool> ActivateAsync(int id, ClaimsPrincipal currentUser);
    public Task<bool> DeactivateAsync(int id, ClaimsPrincipal currentUser);
}