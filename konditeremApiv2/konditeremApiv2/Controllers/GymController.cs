using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace konditeremApiv2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GymController(IGymService service) : ControllerBase
{
    // --- EZT JAVÍTOTTUK: [AllowAnonymous] lett a korlátozás helyett ---
    [HttpGet]
    [AllowAnonymous] 
    public async Task<ActionResult<List<GymResponse>>> Get() => await service.GetAllAsync();

    // --- EZT IS JAVÍTOTTUK ---
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<GymResponse>> Get(int id)
    {
        var gym = await service.GetByIdAsync(id);
        
        return gym is null ? NotFound() : Ok(gym);
    }

    // A TÖBBI MARAD VÉDVE (Csak admin tudjon hozzáadni, módosítani, törölni)
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<GymResponse>> Store(CreateGymRequest request)
    {
        var gym = await service.CreateAsync(request, User);
        
        return gym is null ? NotFound("Gym not found") : NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<GymResponse>> Update(int id, UpdateGymRequest request)
    {
        try
        {
            var isUpdated = await service.UpdateAsync(id, request, User);
            
            return isUpdated ? NoContent() : NotFound("Gym not found");
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<GymResponse>> Delete(int id)
    {
        try
        {
            var isDeleted = await service.DeleteAsync(id, User);
            
            return isDeleted ? NoContent() : NotFound("Gym not found");
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid();
        }
    }
}