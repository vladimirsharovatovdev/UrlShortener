using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Data;
using UrlShortener.Data.Services;
using UrlShortener.DbPopulator;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();


var services = new ServiceCollection();
services.AddSingleton(configuration);
services.AddData(configuration);
services.AddScoped<ManagerService>();
services.AddScoped<DbPopulationService>();
await using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<DbPopulationService>();
await myService.PopulateDb();
