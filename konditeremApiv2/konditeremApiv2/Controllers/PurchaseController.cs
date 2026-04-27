using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace konditeremApiv2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseController(IPurchaseService service) : ControllerBase
{
    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<List<PurchaseResponse>>> GetByUserId(int userId)
    {
        var purchases = await service.GetByUserIdAsync(userId);
        return Ok(purchases);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PurchaseResponse>> Store([FromBody] CreatePurchaseRequest request)
    {
        var purchase = await service.CreateAsync(request, User);

        return purchase is null ? BadRequest("Sikertelen vásárlás") : Ok(purchase);
    }
}