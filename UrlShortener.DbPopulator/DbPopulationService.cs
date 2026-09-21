using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using UrlShortener.Data;
using UrlShortener.Data.Services;

namespace UrlShortener.DbPopulator;

public class DbPopulationService
{
    private readonly UsDbContext _usDbContext;
    private readonly IShortUrlService _shortUrlService;
    private readonly ManagerService _managerService;

    public DbPopulationService(UsDbContext usDbContext, IShortUrlService shortUrlService, ManagerService managerService)
    {
        _usDbContext = usDbContext;
        _shortUrlService = shortUrlService;
        _managerService = managerService;
    }

    public async Task PopulateDb()
    {
        const int maxUrlsCount = 32000000;
        Console.WriteLine($"Beginning population of Database up to {maxUrlsCount} urls.");
        await CreateTestManagers();
        List<Manager> testManagersList = await GetTestManagersAsync();
        await CreateUrls(testManagersList, maxUrlsCount);
    }

    private async Task CreateUrls(List<Manager> testManagersList, int maxUrlsCount)
    {
        const int batchCount = 100;
        int totalRemainingUrlsCount = await RemainingUrlsCount(maxUrlsCount);
        Console.WriteLine($"Creating {totalRemainingUrlsCount} urls.");
        int consoleMessageLength = 0;
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var lastChanged = -1.0;
        
        for (int i = 0; i < totalRemainingUrlsCount; i += batchCount)
        {
            int managerId = (i / batchCount) % testManagersList.Count;
            var modelList = new List<CreateUrlModel>();
            for (int j = 0; j < batchCount; j++)
            {
                Guid randomGuid = Guid.NewGuid();
                var urlModel = new CreateUrlModel
                {
                    LongUrl = randomGuid.ToString()
                };
                modelList.Add(urlModel);
            }
            await _shortUrlService.CreateUrl(modelList, testManagersList[managerId].Id);
            _usDbContext.ChangeTracker.Clear();
            
            var difference = stopWatch.Elapsed.TotalSeconds - lastChanged;

            if (difference > 0.1)
            {
                int createdUrlsCount = i + batchCount;
                int remainingUrlsCount = totalRemainingUrlsCount - createdUrlsCount;
                var remainingSeconds = stopWatch.Elapsed.TotalSeconds / createdUrlsCount * remainingUrlsCount;
                var speed = createdUrlsCount / stopWatch.Elapsed.TotalSeconds;
                lastChanged = stopWatch.Elapsed.TotalSeconds;
                string consoleMessage = $"\r{remainingSeconds:0} seconds remaining to create {remainingUrlsCount} urls {speed:0}/s.";
                string paddedConsoleMessage = consoleMessage.PadRight(consoleMessageLength);
                consoleMessageLength = consoleMessage.Length;
                Console.Write(paddedConsoleMessage);
            }
        }
    }

    private async Task<int> RemainingUrlsCount(int maxUrlsCount)
    {
        int totalUrls = await _usDbContext.Urls.CountAsync();
        int remainingUrls = maxUrlsCount - totalUrls;
        return remainingUrls;
    }

    private async Task CreateTestManagers()
    {
        const string baseString = "TestManager";
        const int addManagersCount = 10;
        int createdManagerCounter = 0;
        Console.WriteLine($"Creating {addManagersCount} managers.");

        for (int i = 1; i <= addManagersCount; i++)
        {
            var name = baseString + i.ToString();
            var password = baseString + i.ToString();
            
            bool isManagerExists = await CheckIfManagerExists(name);

            if (!isManagerExists)
            {
                await _managerService.CreateManager(name, password);
                createdManagerCounter++;
                Console.WriteLine($"Manager {name} created.");
            }
            else
            {
                Console.WriteLine($"Manager {name} already exists.");
            }
        }
        Console.WriteLine($"Created {createdManagerCounter} managers in total.");
    }

    private async Task<bool> CheckIfManagerExists(string name)
    {
        return await _usDbContext.Managers.AnyAsync(m => m.Name == name);
    }

    private async Task<List<Manager>> GetTestManagersAsync()
    {
        return await _usDbContext.Managers
            .AsNoTracking()
            .Where(m => m.Name.Contains("TestManager"))
            .OrderBy(m => m.Name)
            .ToListAsync();
    }
}