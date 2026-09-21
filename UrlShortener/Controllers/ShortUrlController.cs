using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Data.Services;

namespace UrlShortener.Controllers
{
    [ApiController]
    [Route("short-urls")]
    public class ShortUrlController : ControllerBase
    {
        private readonly CurrentUserService _currentUserService;
        private readonly IShortUrlService _shortUrlService;

        public ShortUrlController(CurrentUserService currentUserService, IShortUrlService shortUrlService)
        {
            _currentUserService = currentUserService;
            _shortUrlService = shortUrlService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUrls()
        {
            var currentUser = _currentUserService.GetCurrentUser();
            var managerUrls = await _shortUrlService.GetUrls(currentUser.ManagerId);
            return Ok(managerUrls);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetSingleUrl(int id)
        {
            var currentUser = _currentUserService.GetCurrentUser();
            var url = await _shortUrlService.GetSingleUrl(id, currentUser.ManagerId);
            if (url == null)
                return NotFound();

            return Ok(url);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateUrl([FromBody] CreateUrlModel model)
        {
            model.LongUrl = (model.LongUrl ?? "").Trim();
            model.ShortUrl = model.ShortUrl?.Trim();

            var validationError = UrlValidator.ValidateLongUrl(model.LongUrl)
                                  ?? UrlValidator.ValidateShortUrl(model.ShortUrl);
            if (validationError != null)
                return Problem(detail: validationError, statusCode: StatusCodes.Status400BadRequest);

            var currentUser = _currentUserService.GetCurrentUser();
            var urls = await _shortUrlService.CreateUrl([model], currentUser.ManagerId);

            // The input is valid at this point, so an empty result means the short code is already used.
            if (urls.Count == 0)
                return Problem(detail: "This short code is already taken.", statusCode: StatusCodes.Status400BadRequest);

            return Created((string?)null, value: urls[0]);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUrl(int id)
        {
            var currentUser = _currentUserService.GetCurrentUser();
            var result = await _shortUrlService.DeleteUrl(id, currentUser.ManagerId);
            
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> ChangeUrl(int id, [FromBody] ChangeUrlModel model)
        {
            model.NewLongUrl = (model.NewLongUrl ?? "").Trim();
            model.NewShortUrl = model.NewShortUrl?.Trim();

            var validationError = UrlValidator.ValidateLongUrl(model.NewLongUrl)
                                  ?? UrlValidator.ValidateShortUrl(model.NewShortUrl);
            if (validationError != null)
                return Problem(detail: validationError, statusCode: StatusCodes.Status400BadRequest);

            var currentUser = _currentUserService.GetCurrentUser();
            UrlDto? url;
            
            try
            {
                url = await _shortUrlService.ChangeUrl(id, model, currentUser.ManagerId);
            }
            catch (ArgumentException)
            {
                // Thrown by the service when the short code belongs to another link.
                return Problem(detail: "This short code is already taken.", statusCode: StatusCodes.Status400BadRequest);
            }
            
            if (url == null)
                return NotFound();

            return Ok();
        }
    }
}