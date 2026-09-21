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

    [Theory]
    [InlineData("x' OR '1'='1")]
    [InlineData("abc'; DROP TABLE urls;--")]
    [InlineData("a b")]
    public async Task RedirectUrlReturnsNotFoundForInvalidShortUrlWithoutQueryingService(string shortUrl)
    {
        var testShortUrlService = new TestShortUrlService();
        testShortUrlService.LongUrl = "https://example.com";
        var redirectController = new RedirectController(testShortUrlService);
        var response = await redirectController.RedirectUrl(shortUrl);
        Assert.IsType<NotFoundResult>(response);
        Assert.Equal(0, testShortUrlService.GetLongUrlCallCount);
    }
}

public class TestShortUrlService : IShortUrlService
{
    public string? LongUrl { get; set; }
    public int GetLongUrlCallCount { get; private set; }
    public async Task<string?> GetLongUrl(string shortUrl)
    {
        GetLongUrlCallCount++;
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