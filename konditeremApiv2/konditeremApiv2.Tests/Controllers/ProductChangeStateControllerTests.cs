using konditeremApiv2.Controllers;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace konditeremApiv2.Tests.Controllers;

public class ProductChangeStateControllerTests
{
    private static ProductChangeStateController CreateController(Mock<IProductService> serviceMock)
    {
        var controller = new ProductChangeStateController(serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "admin")], "test"))
                }
            }
        };

        return controller;
    }

    [Fact]
    public async Task Activate_ReturnsNoContent_WhenSuccessful()
    {
        var serviceMock = new Mock<IProductService>();
        serviceMock.Setup(s => s.ActivateAsync(1, It.IsAny<ClaimsPrincipal>())).ReturnsAsync(false);
        var controller = CreateController(serviceMock);

        var result = await controller.Activate(1);

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Activate_ReturnsNotFound_WhenMissing()
    {
        var serviceMock = new Mock<IProductService>();
        serviceMock.Setup(s => s.ActivateAsync(99, It.IsAny<ClaimsPrincipal>())).ReturnsAsync(true);
        var controller = CreateController(serviceMock);

        var result = await controller.Activate(99);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Deactivate_ReturnsNoContent_WhenSuccessful()
    {
        var serviceMock = new Mock<IProductService>();
        serviceMock.Setup(s => s.DeactivateAsync(1, It.IsAny<ClaimsPrincipal>())).ReturnsAsync(false);
        var controller = CreateController(serviceMock);

        var result = await controller.Deactivate(1);

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Deactivate_ReturnsNotFound_WhenMissing()
    {
        var serviceMock = new Mock<IProductService>();
        serviceMock.Setup(s => s.DeactivateAsync(77, It.IsAny<ClaimsPrincipal>())).ReturnsAsync(true);
        var controller = CreateController(serviceMock);

        var result = await controller.Deactivate(77);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
