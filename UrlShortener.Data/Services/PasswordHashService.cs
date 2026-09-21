using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace UrlShortener.Data.Services;

public class PasswordHashService(IConfiguration configuration)
{
    private string GetSecret()
    {
        return configuration.GetValue<string>("PasswordSecret");
    }
    public string HashPassword(string password, string name)
    {
        string payload = password + name;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(GetSecret()));
        byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(signature);
    }
}