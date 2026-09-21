using Microsoft.AspNetCore.Mvc;
using UrlShortener.Controllers;
using UrlShortener.Data.Services;
using Xunit;

namespace UrlShortener.Tests;

public class RedirectControllerTests
{
    [Fact]
    public async Task RedirectUrlReturnsUrlStatus()
    {
        var testShortUrlService = new TestShortUrlService();
        testShortUrlService.LongUrl = "asdfasdf89ya9sydfga09sdyfa09s8dfy";
        var redirectController = new RedirectController(testShortUrlService);
        var response = await redirectController.RedirectUrl("asdufi");
        Assert.IsType<RedirectResult>(response);
        var redirectResult = (RedirectResult)response;
        Assert.Equal("asdfasdf89ya9sydfga09sdyfa09s8dfy", redirectResult.Url);
    }
    
    [Fact]
    public async Task RedirectUrlReturnsNotFound()
    {
        var testShortUrlService = new TestShortUrlService();
        testShortUrlService.LongUrl = null;
        var redirectController = new RedirectController(testShortUrlService);
        var response = await redirectController.RedirectUrl("asdufi");
        Assert.IsType<NotFoundResult>(response);
    }
}

public class TestShortUrlService : IShortUrlService
{
    public string? LongUrl { get; set; }
    public async Task<string?> GetLongUrl(string shortUrl)
    {
        return LongUrl;
    }

    public async Task<List<UrlDto>> CreateUrl(List<CreateUrlModel> models, int managerId)
    {
        return [];
    }

    public Task<List<UrlDto>> GetUrls(int managerId)
    {
        throw new NotImplementedException();
    }

    public Task<UrlDto?> GetSingleUrl(int urlId, int managerId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteUrl(int id, int managerId)
    {
        throw new NotImplementedException();
    }

    public Task<UrlDto?> ChangeUrl(int id, ChangeUrlModel model, int managerId)
    {
        throw new NotImplementedException();
    }
}