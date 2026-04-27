using konditeremApiv2.Controllers;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace konditeremApiv2.Tests.Controllers;

public class ProductControllerTests
{
    private static ClaimsPrincipal CreatePrincipal(int id, string role)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, role)],
            "test"));
    }

    private static ProductController CreateController(Mock<IProductService> serviceMock, ClaimsPrincipal? principal = null)
    {
        var controller = new ProductController(serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal ?? CreatePrincipal(1, "admin") }
            }
        };

        return controller;
    }

    [Fact]
    public async Task Get_ReturnsProducts()
    {
        var serviceMock = new Mock<IProductService>();
        serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync([new ProductResponse { Id = 1, Designation = "Monthly", IsActive = true }]);
        var controller = CreateController(serviceMock);

        var result = await controller.Get();

        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var serviceMock = new Mock<IProductService>();
        serviceMock.Setup(s => s.GetByIdAsync(3)).ReturnsAsync((ProductResponse?)null);
        var controller = CreateController(serviceMock);

        var result = await controller.Get(3);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Store_ReturnsNoContent_WhenCreated()
    {
        var serviceMock = new Mock<IProductService>();
        serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateProductRequest>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new ProductResponse { Id = 1, Designation = "Monthly", IsActive = true });
        var controller = CreateController(serviceMock);

        var result = await controller.Store(new CreateProductRequest { Designation = "Monthly", GymIds = [1] });

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMissing()
    {
        var serviceMock = new Mock<IProductService>();
        serviceMock.Setup(s => s.UpdateAsync(8, It.IsAny<UpdateProductRequest>(), It.IsAny<ClaimsPrincipal>())).ReturnsAsync(false);
        var controller = CreateController(serviceMock);

        var result = await controller.Update(8, new UpdateProductRequest
        {
            Id = 8,
            Designation = "Updated",
            GymIds = [1],
            IsActive = true
        });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsForbid_WhenUnauthorized()
    {
        var serviceMock = new Mock<IProductService>();
        serviceMock.Setup(s => s.DeleteAsync(9, It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new UnauthorizedAccessException());
        var controller = CreateController(serviceMock);

        var result = await controller.Delete(9);

        Assert.IsType<ForbidResult>(result.Result);
    }
}
