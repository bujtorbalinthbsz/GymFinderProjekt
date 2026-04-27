using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace konditeremApiv2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController(IProductService service) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<List<ProductResponse>>> Get() => await service.GetAllAsync();

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductResponse>> Get(int id)
    {
        var product = await service.GetByIdAsync(id);
        
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductResponse>> Store(CreateProductRequest request)
    {
        var product = await service.CreateAsync(request, User);
        
        return product is null ? NotFound("Product not found") : NoContent();
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest request)
    {
        try
        {
            var isUpdated = await service.UpdateAsync(id, request, User);
            
            return isUpdated ? NoContent() : NotFound("Product not found");
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid();
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ProductResponse>> Delete(int id)
    {
        try
        {
            var isDeleted = await service.DeleteAsync(id, User);
            
            return isDeleted ? NoContent() : NotFound("Product not found");
        }
        catch (UnauthorizedAccessException e)
        {
            return Forbid();
        }
    }
}