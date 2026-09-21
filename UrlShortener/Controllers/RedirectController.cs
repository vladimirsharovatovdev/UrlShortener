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
        var longUrl = await shortUrlService.GetLongUrl(shortUrl);
        if(longUrl == null)
            return NotFound();
        return Redirect(longUrl);
    }
}