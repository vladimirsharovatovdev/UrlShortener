using UrlShortener.Data.Services;
using Xunit;

namespace UrlShortener.Tests;

public class UrlValidatorTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("abc")]
    [InlineData("aB3xK9")]
    [InlineData("my-link_2026")]
    public void IsValidShortUrlAcceptsSafeCodes(string shortUrl)
    {
        Assert.True(UrlValidator.IsValidShortUrl(shortUrl));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a b")]
    [InlineData("a/b")]
    [InlineData("a?b=1")]
    [InlineData("a.b")]
    [InlineData("x' OR '1'='1")]
    [InlineData("abc'; DROP TABLE urls;--")]
    [InlineData("привет")]
    [InlineData("abc\n")]
    public void IsValidShortUrlRejectsUnsafeCodes(string? shortUrl)
    {
        Assert.False(UrlValidator.IsValidShortUrl(shortUrl));
    }

    [Fact]
    public void IsValidShortUrlEnforcesMaxLength()
    {
        Assert.True(UrlValidator.IsValidShortUrl(new string('a', UrlValidator.MaxShortUrlLength)));
        Assert.False(UrlValidator.IsValidShortUrl(new string('a', UrlValidator.MaxShortUrlLength + 1)));
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://localhost:5158/path?x=1&y=2#frag")]
    [InlineData("https://sub.example.com/a%20b/c")]
    public void IsValidLongUrlAcceptsHttpAndHttpsUrls(string longUrl)
    {
        Assert.True(UrlValidator.IsValidLongUrl(longUrl));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("example.com")]
    [InlineData("//example.com")]
    [InlineData("https://")]
    [InlineData("ftp://example.com/file")]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,<script>alert(1)</script>")]
    [InlineData("https://exa mple.com")]
    [InlineData("https://example.com/a\nb")]
    public void IsValidLongUrlRejectsOtherInput(string? longUrl)
    {
        Assert.False(UrlValidator.IsValidLongUrl(longUrl));
    }

    [Fact]
    public void IsValidLongUrlEnforcesMaxLength()
    {
        var tooLong = "https://example.com/" + new string('a', UrlValidator.MaxLongUrlLength);
        Assert.False(UrlValidator.IsValidLongUrl(tooLong));
        Assert.NotNull(UrlValidator.ValidateLongUrl(tooLong));
    }

    [Fact]
    public void ValidateShortUrlAllowsEmptyCodeForAutoGeneration()
    {
        Assert.Null(UrlValidator.ValidateShortUrl(null));
        Assert.Null(UrlValidator.ValidateShortUrl(""));
    }

    [Fact]
    public void ValidateReturnsMessagesForInvalidInput()
    {
        Assert.NotNull(UrlValidator.ValidateShortUrl("a b"));
        Assert.NotNull(UrlValidator.ValidateLongUrl("nope"));
        Assert.Null(UrlValidator.ValidateShortUrl("ok-code"));
        Assert.Null(UrlValidator.ValidateLongUrl("https://example.com"));
    }
}
