using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;

namespace konditeremApiv2.Interfaces.Services;

public interface IAuthService
{
    public Task<LoginResponse?> LoginAsync(LoginRequest request);
    public Task<UserResponse?> RegisterAsync(RegisterRequest request);
    public Task LogoutAsync();
}