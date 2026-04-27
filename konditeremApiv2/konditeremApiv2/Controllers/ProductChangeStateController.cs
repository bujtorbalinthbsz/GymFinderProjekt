using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace konditeremApiv2.Controllers;

[ApiController]
[Route("/api/Product/Change-state")]
public class ProductChangeStateController(IProductService service) : ControllerBase
{
    [HttpGet("{id}/Activate")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductResponse>> Activate(int id)
        => await service.ActivateAsync(id, User) ? NotFound("Product not found") : NoContent();
    
    [HttpGet("{id}/Deactivate")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductResponse>> Deactivate(int id)
        => await service.DeactivateAsync(id, User) ? NotFound("Product not found") : NoContent();
}