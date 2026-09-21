using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data.Services;

namespace UrlShortener.Controllers;

[ApiController]
[Route("r")]
public class RedirectController(IShortUrlService shortUrlService) : ControllerBase
{
    [HttpGet("{shortUrl}")]
    public async Task<IActionResult> RedirectUrl(string shortUrl)
    {
        // Reject malformed codes early: they can never exist, and it keeps junk out of the database layer.
        if (!UrlValidator.IsValidShortUrl(shortUrl))
            return NotFound();

        var longUrl = await shortUrlService.GetLongUrl(shortUrl);
        if(longUrl == null)
            return NotFound();
        return Redirect(longUrl);
    }
}