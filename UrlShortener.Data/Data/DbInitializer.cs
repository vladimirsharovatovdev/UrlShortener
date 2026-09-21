using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Data;

public static class DbInitializer
{
    public static void Initialize(UsDbContext context)
    {
        context.Database.Migrate();
    }
}