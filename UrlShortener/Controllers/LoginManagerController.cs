using Microsoft.AspNetCore.Mvc;
using UrlShortener.Data.Services;

namespace UrlShortener.Controllers;

[ApiController]
[Route("manager-login")]
public class LoginManagerController : ControllerBase
{
    private readonly IManagerAuthService _managerAuthService;
    public LoginManagerController (IManagerAuthService managerAuthService)
    {
        _managerAuthService = managerAuthService;
    }

    [HttpPost]
    public async Task<IActionResult> LoginManager([FromBody] LoginManagerModel model)
    {
        var managerTokens = await _managerAuthService.LoginManager(model.Name, model.Password);

        if (managerTokens == null)
        {
            return NotFound();
        }

        return Ok(managerTokens);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
    {
        var managerTokensDto = await _managerAuthService.RefreshToken(model.AuthToken, model.RefreshToken);
        if (managerTokensDto == null)
        {
            return NotFound();
        }
        return Ok(managerTokensDto);
    }
}

public class LoginManagerModel
{
    public required string Name { get; init; }
    public required string Password { get; init; }
}

public class RefreshTokenModel
{
    public required string AuthToken { get; init; }
    public required string RefreshToken { get; init; }
}