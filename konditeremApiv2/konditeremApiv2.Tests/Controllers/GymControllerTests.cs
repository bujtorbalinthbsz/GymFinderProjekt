using konditeremApiv2.Controllers;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Text.Json.Nodes;

namespace konditeremApiv2.Tests.Controllers;

public class GymControllerTests
{
    private static ClaimsPrincipal CreatePrincipal(int id, string role)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, role)],
            "test"));
    }

    private static GymController CreateController(Mock<IGymService> serviceMock, ClaimsPrincipal? principal = null)
    {
        var controller = new GymController(serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal ?? CreatePrincipal(1, "admin") }
            }
        };

        return controller;
    }

    [Fact]
    public async Task Get_ReturnsGyms()
    {
        var serviceMock = new Mock<IGymService>();
        serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync([new GymResponse { Id = 1, Name = "Gym", OpenAt = JsonNode.Parse("{}")!.AsObject() }]);
        var controller = CreateController(serviceMock);

        var result = await controller.Get();

        Assert.Single(result.Value!);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenExists()
    {
        var serviceMock = new Mock<IGymService>();
        serviceMock.Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(new GymResponse { Id = 1, Name = "Gym", OpenAt = JsonNode.Parse("{}")!.AsObject() });
        var controller = CreateController(serviceMock);

        var result = await controller.Get(1);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Store_ReturnsNotFound_WhenServiceReturnsNull()
    {
        var serviceMock = new Mock<IGymService>();
        serviceMock.Setup(s => s.CreateAsync(It.IsAny<CreateGymRequest>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((GymResponse?)null);
        var controller = CreateController(serviceMock);

        var result = await controller.Store(new CreateGymRequest { Name = "Gym", OpenAt = JsonNode.Parse("{}")!.AsObject() });

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenUpdated()
    {
        var serviceMock = new Mock<IGymService>();
        serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<UpdateGymRequest>(), It.IsAny<ClaimsPrincipal>())).ReturnsAsync(true);
        var controller = CreateController(serviceMock);

        var result = await controller.Update(1,
            new UpdateGymRequest { Id = 1, Name = "Gym", OpenAt = JsonNode.Parse("{}")!.AsObject() });

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsForbid_WhenUnauthorized()
    {
        var serviceMock = new Mock<IGymService>();
        serviceMock.Setup(s => s.DeleteAsync(2, It.IsAny<ClaimsPrincipal>())).ThrowsAsync(new UnauthorizedAccessException());
        var controller = CreateController(serviceMock);

        var result = await controller.Delete(2);

        Assert.IsType<ForbidResult>(result.Result);
    }
}
