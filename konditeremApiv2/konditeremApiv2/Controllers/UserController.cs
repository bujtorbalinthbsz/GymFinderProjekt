using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace konditeremApiv2.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "admin")]
public class UserController(IUserService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "admin,user")]
    public async Task<ActionResult<List<UserResponse>>> Get() => Ok(await service.GetAllAsync());

    [HttpGet("me")]
    [Authorize(Roles = "admin,user")]
    public async Task<ActionResult<UserResponse>> GetCurrentUser()
    {
        try
        {
            var user = await service.GetCurrentAsync(User);
            return user is null ? NotFound("User not found") : Ok(user);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(exception.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,user")]
    public async Task<ActionResult<UserResponse>> Get(int id)
    {
        var user = await service.GetByIdAsync(id);

        return user is  null ? NotFound("User not found") : Ok(user);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Update(int id, UpdateUserRequest request)
    {
        try
        {
            var isUpdated = await service.UpdateAsync(id, request, User);
            return isUpdated ? NoContent() : NotFound("User not found");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> Delete(int id)
    {
        try
        {
            var isDeleted  = await service.DeleteAsync(id, User);
            return isDeleted ? NoContent() : NotFound("User not found");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}