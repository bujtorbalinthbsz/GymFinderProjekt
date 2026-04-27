using konditeremApiv2.Controllers;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace konditeremApiv2.Tests.Controllers;

public class UserControllerTests
{
    private static ClaimsPrincipal CreatePrincipal(int id, string role)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, role)],
            "test"));
    }

    private static UserController CreateController(Mock<IUserService> serviceMock, ClaimsPrincipal? principal = null)
    {
        var controller = new UserController(serviceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal ?? CreatePrincipal(1, "admin") }
            }
        };

        return controller;
    }

    [Fact]
    public async Task Get_ReturnsOk_WithUsers()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(
            [new UserResponse { Id = 1, Name = "A", Email = "a@test.com", Role = "admin" }]);
        var controller = CreateController(serviceMock);

        var result = await controller.Get();

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync((UserResponse?)null);
        var controller = CreateController(serviceMock);

        var result = await controller.Get(5);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsUnauthorized_WhenServiceThrowsUnauthorized()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.GetCurrentAsync(It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid token subject."));
        var controller = CreateController(serviceMock);

        var result = await controller.GetCurrentUser();

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenUpdated()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.UpdateAsync(2, It.IsAny<UpdateUserRequest>(), It.IsAny<ClaimsPrincipal>())).ReturnsAsync(true);
        var controller = CreateController(serviceMock);

        var result = await controller.Update(2, new UpdateUserRequest { Id = 2, Name = "U", Email = "u@test.com", Role = "user" });

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsForbid_WhenUnauthorized()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.UpdateAsync(3, It.IsAny<UpdateUserRequest>(), It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        var controller = CreateController(serviceMock, CreatePrincipal(2, "user"));

        var result = await controller.Update(3, new UpdateUserRequest { Id = 3, Name = "X", Email = "x@test.com", Role = "user" });

        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.DeleteAsync(2, It.IsAny<ClaimsPrincipal>())).ReturnsAsync(true);
        var controller = CreateController(serviceMock);

        var result = await controller.Delete(2);

        Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ReturnsForbid_WhenUnauthorized()
    {
        var serviceMock = new Mock<IUserService>();
        serviceMock.Setup(s => s.DeleteAsync(3, It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        var controller = CreateController(serviceMock, CreatePrincipal(2, "user"));

        var result = await controller.Delete(3);

        Assert.IsType<ForbidResult>(result.Result);
    }
}
