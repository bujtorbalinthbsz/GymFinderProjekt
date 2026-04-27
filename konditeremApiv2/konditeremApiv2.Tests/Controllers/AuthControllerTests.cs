using konditeremApiv2.Controllers;
using konditeremApiv2.DTOs.Requests;
using konditeremApiv2.DTOs.Responses;
using konditeremApiv2.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace konditeremApiv2.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_ReturnsOk_WhenServiceReturnsToken()
    {
        var serviceMock = new Mock<IAuthService>();
        serviceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync(new LoginResponse { Token = "jwt-token" });

        var controller = new AuthController(serviceMock.Object);
        var result = await controller.Login(new LoginRequest { Email = "a@a.com", Password = "p" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<LoginResponse>(okResult.Value);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenServiceReturnsNull()
    {
        var serviceMock = new Mock<IAuthService>();
        serviceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
            .ReturnsAsync((LoginResponse?)null);

        var controller = new AuthController(serviceMock.Object);
        var result = await controller.Login(new LoginRequest { Email = "a@a.com", Password = "wrong" });

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Register_ReturnsOk_WhenServiceCreatesUser()
    {
        var serviceMock = new Mock<IAuthService>();
        serviceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync(new UserResponse { Id = 1, Name = "Test", Email = "test@test.com", Role = "user" });

        var controller = new AuthController(serviceMock.Object);
        var result = await controller.Register(new RegisterRequest { Name = "Test", Email = "test@test.com", Password = "p" });

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<UserResponse>(okResult.Value);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var serviceMock = new Mock<IAuthService>();
        serviceMock
            .Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
            .ReturnsAsync((UserResponse?)null);

        var controller = new AuthController(serviceMock.Object);
        var result = await controller.Register(new RegisterRequest { Name = "Test", Email = "test@test.com", Password = "p" });

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Logout_ReturnsNoContent_AndCallsService()
    {
        var serviceMock = new Mock<IAuthService>();
        serviceMock
            .Setup(s => s.LogoutAsync())
            .Returns(Task.CompletedTask);

        var controller = new AuthController(serviceMock.Object);
        var result = await controller.Logout();

        Assert.IsType<NoContentResult>(result);
        serviceMock.Verify(s => s.LogoutAsync(), Times.Once);
    }
}
