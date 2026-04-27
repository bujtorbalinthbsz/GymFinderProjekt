using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace konditeremApiv2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CityController(ICityService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<List<CityResponse>>> Get() => await service.GetAllAsync();

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CityResponse>> Get(int id)
    {
        var product = await service.GetByIdAsync(id);
        
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CityResponse>> Store(CreateCityRequest request)
    {
        var product = await service.CreateAsync(request, User);
        
        return product is null ? NotFound("City not found") : NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CityResponse>> Update(int id, UpdateCityRequest request)
    {
        try
        {
            var isUpdated = await service.UpdateAsync(id, request, User);
            
            return isUpdated ? NoContent() : NotFound("City not found");
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<CityResponse>> Delete(int id)
    {
        try
        {
            var isDeleted = await service.DeleteAsync(id, User);
            
            return isDeleted ? NoContent() : NotFound("City not found");
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid();
        }
    }
}