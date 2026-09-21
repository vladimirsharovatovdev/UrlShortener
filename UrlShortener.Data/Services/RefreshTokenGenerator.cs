using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace UrlShortener.Data.Services;

public class RefreshTokenGenerator
{
    private readonly IConfiguration _configuration;
    public RefreshTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    private AuthSettings GetSettings()
    {
        return _configuration.GetRequiredSection("Auth").Get<AuthSettings>();
    }
    public string GenerateRefreshToken()
    {
        Guid guid = Guid.NewGuid();
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(GetSettings().Secret));
        byte[] signature = hmac.ComputeHash(guid.ToByteArray());
        return Convert.ToBase64String(signature);
    }
}