using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Data.Services;

namespace UrlShortener.Data;

public static class DiExtensions
{
    public static void AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsDbContext>(options =>
            options.UseNpgsql(configuration
                    .GetConnectionString("Default"))
                .UseSnakeCaseNamingConvention());
        services.AddSingleton<RefreshTokenGenerator>();
        services.AddSingleton<PasswordHashService>();
        services.AddSingleton<JwtService>();
        services.AddScoped<IShortUrlService, ShortUrlService>();
        services.AddScoped<IManagerAuthService, ManagerAuthService>();
    }
}