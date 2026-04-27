using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace konditeremApiv2.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var loginResponse = await service.LoginAsync(request);

        return loginResponse is null ? Unauthorized("Invalid email or password") : Ok(loginResponse);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterRequest request)
    {
        var user = await service.RegisterAsync(request);

        return user is null ? Conflict("Email is already in use") : Ok(user);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await service.LogoutAsync();
        return NoContent();
    }
}