using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;

namespace UrlShortener.Data.Services;

public class ManagerAuthService : IManagerAuthService
{
    private PasswordHashService _passwordHashService;
    private UsDbContext _dbContext;
    private JwtService _jwtService;
    private RefreshTokenGenerator _refreshTokenGenerator;

    public ManagerAuthService(PasswordHashService passwordHashService, UsDbContext usDbContext, 
        JwtService jwtService, RefreshTokenGenerator refreshTokenGenerator)
    {
        _passwordHashService = passwordHashService;
        _dbContext = usDbContext;
        _jwtService = jwtService;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<ManagerTokensDto?> LoginManager(string name, string password)
    {
        var passwordHash = _passwordHashService.HashPassword(password, name);
        var manager = await _dbContext.Managers
            .FirstOrDefaultAsync(x => x.Name == name && x.PasswordHash == passwordHash);

        if (manager == null)
        {
            return null;
        }
        return await GenerateTokens(manager.Id, manager.Name);
    }
    
    public async Task<ManagerTokensDto?> RefreshToken(string authToken, string refreshToken)
    {
        RefreshTokenEntry? tokenEntry = await _dbContext.RefreshTokens
            .Include(x => x.Manager)
            .FirstOrDefaultAsync(x => x.AuthToken == authToken && x.RefreshToken == refreshToken);
        if (tokenEntry == null)
        {
            return null;
        }
        return await GenerateTokens(tokenEntry.ManagerId, tokenEntry.Manager.Name);
    }
    
    private async Task<ManagerTokensDto> GenerateTokens(int managerId, string name)
    {
        var tokenEntries = await _dbContext.RefreshTokens
            .Where(x => x.ManagerId == managerId)
            .ToListAsync();
        _dbContext.RemoveRange(tokenEntries);
        string token = _jwtService.GenerateToken(managerId, name);
        string refreshToken = _refreshTokenGenerator.GenerateRefreshToken();
        var refreshTokenEntry = new RefreshTokenEntry
            { ManagerId = managerId, AuthToken = token, RefreshToken = refreshToken };
        _dbContext.Add(refreshTokenEntry);
        await _dbContext.SaveChangesAsync();
        return new ManagerTokensDto { Token = token, RefreshToken = refreshToken };
    }
}

public interface IManagerAuthService
{
    Task<ManagerTokensDto?> LoginManager(string name, string password);
    Task<ManagerTokensDto?> RefreshToken(string authToken, string refreshToken);
}

public class ManagerTokensDto
{
    public required string Token { get; init; }
    public required string RefreshToken {get; init;}
}