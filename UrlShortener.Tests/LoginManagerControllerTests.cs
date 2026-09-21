using Microsoft.AspNetCore.Mvc;
using UrlShortener.Controllers;
using UrlShortener.Data.Services;
using Xunit;

namespace UrlShortener.Tests;

public class LoginManagerControllerTests
{
    [Fact]
    public async Task LoginReturnsOk()
    {
        var testManagerAuthService = new TestManagerAuthService();
        testManagerAuthService.LoginManagerOverride = (name, password) =>
        {
            Assert.Equal("Aboba", name);
            Assert.Equal("Amogus", password);
            return new ManagerTokensDto { Token = "123", RefreshToken = "456" };
        };
        var loginManagerController = new LoginManagerController(testManagerAuthService);
        var response = await loginManagerController.LoginManager(new LoginManagerModel { Name = "Aboba", Password = "Amogus" });
        Assert.IsType<OkObjectResult>(response);
        var responseResult = (OkObjectResult)response;
        var test = new ManagerTokensDto() {Token = "123", RefreshToken = "456"};
        Assert.Equivalent(test, responseResult.Value);
    }
    
    [Fact]
    public async Task LoginReturnsNotFound()
    {
        var testManagerAuthService = new TestManagerAuthService();
        testManagerAuthService.LoginManagerOverride = (name, password) => null;
        var loginManagerController = new LoginManagerController(testManagerAuthService);
        var response = await loginManagerController.LoginManager(new LoginManagerModel { Name = "Aboba", Password = "Amogus" });
        Assert.IsType<NotFoundResult>(response);
    }
    
    [Fact]
    public async Task RefreshTokenReturnsOk()
    {
        var testManagerAuthService = new TestManagerAuthService();
        testManagerAuthService.RefreshTokenOverride = (name, password) =>
        {
            Assert.Equal("Aboba", name);
            Assert.Equal("Amogus", password);
            return new ManagerTokensDto { Token = "123", RefreshToken = "456" };
        };
        var loginManagerController = new LoginManagerController(testManagerAuthService);
        var response = await loginManagerController.RefreshToken(new RefreshTokenModel { AuthToken = "Aboba", RefreshToken = "Amogus" });
        Assert.IsType<OkObjectResult>(response);
        var responseResult = (OkObjectResult)response;
        var test = new ManagerTokensDto() {Token = "123", RefreshToken = "456"};
        Assert.Equivalent(test, responseResult.Value);
    }
    
    [Fact]
    public async Task RefreshTokenReturnsNotFound()
    {
        var testManagerAuthService = new TestManagerAuthService();
        testManagerAuthService.RefreshTokenOverride = (name, password) => null;
        var loginManagerController = new LoginManagerController(testManagerAuthService);
        var response = await loginManagerController.RefreshToken(new RefreshTokenModel() { AuthToken = "Aboba", RefreshToken = "Amogus" });
        Assert.IsType<NotFoundResult>(response);
    }
}

public class TestManagerAuthService : IManagerAuthService
{
    public Func<string, string, ManagerTokensDto?>? LoginManagerOverride { get; set; }
    public Task<ManagerTokensDto?> LoginManager(string name, string password)
    {
        if (LoginManagerOverride == null)
            return Task.FromResult<ManagerTokensDto?>(null);
        
        var result = LoginManagerOverride(name, password);
        return Task.FromResult(result);
    }
    public Func<string, string, ManagerTokensDto?>? RefreshTokenOverride { get; set; }
    public Task<ManagerTokensDto?> RefreshToken(string authToken, string refreshToken)
    {
        if (RefreshTokenOverride == null)
            return Task.FromResult<ManagerTokensDto?>(null);
        
        var result = RefreshTokenOverride(authToken, refreshToken);
        return Task.FromResult(result);
    }
}