using konditeremApiv2.Controllers;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace konditeremApiv2.Tests.Controllers;

public class CityControllerTests
{
    private static ClaimsPrincipal CreatePrincipal(int id, string role)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, role)],
            "test"));
    }

    private static CityController CreateController(Mock<ICityService> serviceMock, ClaimsPrincipal? principal = null)
    {
        var controller = new CityController(serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal ?? CreatePrincipal(1, "admin") }
            }
        };

        return controller;
    }

    [Fact]
    public async Task Get_ReturnsCities()
    {
        var serviceMock = new Mock<ICityService>();
        serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync([new CityResponse { Id = 1, Name = "Budapest", ZipCode = 1111 }]);
        var controller = CreateController(serviceMock);

        var result = await controller.Get();

        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var serviceMock = new Mock<ICityService>();
        serviceMock.Setup(s => s.GetByIdAsync(9)).ReturnsAsync((CityResponse?)null);
        var controller = CreateController(serviceMock);

        var result = await controller.Get(9);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Store_ReturnsNoContent_WhenCreated()
    {
        var serviceMock = new Mock<ICityService>();
        serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateCityRequest>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new CityResponse { Id = 1, Name = "Budapest", ZipCode = 1111 });
        var controller = CreateController(serviceMock);

        var result = await controller.Store(new CreateCityRequest { Name = "Budapest", ZipCode = 1111 });

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsForbid_WhenUnauthorized()
    {
        var serviceMock = new Mock<ICityService>();
        serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateCityRequest>(), It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        var controller = CreateController(serviceMock);

        var result = await controller.Update(1, new UpdateCityRequest { Id = 1, Name = "X", ZipCode = 1111 });

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var serviceMock = new Mock<ICityService>();
        serviceMock.Setup(s => s.DeleteAsync(7, It.IsAny<ClaimsPrincipal>())).ReturnsAsync(false);
        var controller = CreateController(serviceMock);

        var result = await controller.Delete(7);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
