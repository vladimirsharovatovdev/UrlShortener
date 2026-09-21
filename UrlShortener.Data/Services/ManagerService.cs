using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Data.Services;

public class ManagerService
{
    private readonly UsDbContext _dbContext;
    private readonly PasswordHashService _passwordHashService;
    
    public ManagerService(UsDbContext usDbContext, PasswordHashService passwordHashService)
    {
        _dbContext = usDbContext;
        _passwordHashService = passwordHashService;
    }
    
    public async Task<List<ManagerDto>> GetManagers()
    {
        return await _dbContext.Managers.ToManagerDto().ToListAsync();
    }
    
    public async Task<ManagerDto?> CreateManager(string name, string password)
    {
        bool nameIsNotUnique = await _dbContext.Managers.AnyAsync(x => x.Name == name);
        if (String.IsNullOrEmpty(name) || nameIsNotUnique)
        {
            return null;
        }

        if (String.IsNullOrEmpty(password))
        {
            return null;
        }
        var passwordHash = _passwordHashService.HashPassword(password, name);
        var manager = new Manager
        {
            Name = name,
            PasswordHash = passwordHash
        };

        _dbContext.Add(manager);

        await _dbContext.SaveChangesAsync();

        return manager.ToManagerDto();
    }
    
    public async Task<ManagerDto?> UpdateCredentials(string password, string? newName, string? newPassword, int managerId)
    {
        var manager = await _dbContext.Managers.FirstOrDefaultAsync(x => x.Id == managerId);
        bool isManagerChanged = false;
        if (manager == null)
        {
            return null;
        }
        var modelHashPassword = _passwordHashService.HashPassword(password, manager.Name);
        if (manager.PasswordHash != modelHashPassword)
        {
            throw new UnauthorizedAccessException();
        }
        if (!String.IsNullOrEmpty(newName) && manager.Name != newName)
        {
            bool nameIsNotUnique = await _dbContext.Managers.AnyAsync(x => x.Name == newName);
            if(!nameIsNotUnique)
            {
                manager.Name = newName;
                modelHashPassword = _passwordHashService.HashPassword(password, manager.Name);
                manager.PasswordHash = modelHashPassword;
                isManagerChanged = true;
            }
        }
        if (!String.IsNullOrEmpty(newPassword))
        {
            var modelNewHashPassword = _passwordHashService.HashPassword(newPassword, manager.Name);
            if (manager.PasswordHash != modelNewHashPassword)
            {
                manager.PasswordHash = modelNewHashPassword;
                isManagerChanged = true;
            }
        }
        if (isManagerChanged)
        {
            await _dbContext.SaveChangesAsync();
        }
        return manager.ToManagerDto();
    }
    
    public async Task<bool> DeleteManager(int managerId)
    {
        var manager = await _dbContext.Managers
            .Include(x => x.AssociatedUrls)
            .FirstOrDefaultAsync(x => x.Id == managerId);
        if (manager == null)
            return false;

        _dbContext.Remove(manager);

        await _dbContext.SaveChangesAsync();

        return true;
    }
}

public class ManagerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class CreateManagerModel
{
    public string Name { get; set; }
    public string Password { get; set; }
}

public class UpdateManagerCredentialsModel
{
    public string Password { get; set; }
    public string? NewName { get; set; }
    public string? NewPassword { get; set; }
}

public static class ManagerDtoExtensions
{
    public static IQueryable<ManagerDto> ToManagerDto(this IQueryable<Manager> managers)
    {
        return managers.Select(x => new ManagerDto
        {
            Id = x.Id,
            Name = x.Name
        });
    }

    public static ManagerDto ToManagerDto(this Manager manager)
    {
        return new ManagerDto
        {
            Id = manager.Id,
            Name = manager.Name
        };
    }
}
