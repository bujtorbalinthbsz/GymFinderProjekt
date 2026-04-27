using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace konditeremApiv2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatingController(IRatingService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RatingResponse>>> Get()
    {
        var ratings = await service.GetAllAsync();
        return Ok(ratings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RatingResponse>> Get(int id)
    {
        var rating = await service.GetByIdAsync(id);
        return rating is null ? NotFound() : Ok(rating);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RatingResponse>> Store([FromBody] CreateRatingRequest request)
    {
        var rating = await service.CreateAsync(request, User);
        return rating is null ? BadRequest("Sikertelen értékelés küldés") : Ok(rating);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRatingRequest request)
    {
        var isUpdated = await service.UpdateAsync(id, request, User);
        return isUpdated ? NoContent() : BadRequest("Sikertelen módosítás");
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var isDeleted = await service.DeleteAsync(id, User);
        return isDeleted ? NoContent() : BadRequest("Sikertelen törlés");
    }
}