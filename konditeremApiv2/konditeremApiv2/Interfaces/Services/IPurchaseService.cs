using System.Security.Claims;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Interfaces.Services;

public interface IPurchaseService
{
    Task<List<PurchaseResponse>> GetAllAsync();
    Task<List<PurchaseResponse>> GetByUserIdAsync(int userId);
    Task<PurchaseResponse?> GetByIdAsync(Guid id);
    Task<PurchaseResponse?> CreateAsync(CreatePurchaseRequest request, ClaimsPrincipal currentUser);
    Task<bool> UpdateAsync(Guid id, UpdatePurchaseRequest request, ClaimsPrincipal currentUser);
    Task<bool> DeleteAsync(Guid id, ClaimsPrincipal currentUser);
}