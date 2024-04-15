using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;
using ActionEntity = AccessControl.API.Entities.Action;
using AccessControl.API.Entities;

namespace AccessControl.API.Persistence
{
    public class AccessControlContextSeed
    {
        private readonly ILogger _logger;
        private readonly AccessControlContext _context;

        public AccessControlContextSeed(ILogger logger, AccessControlContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                if (_context.Database.IsSqlServer())
                {
                    await _context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await TrySeedAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task TrySeedAsync()
        {
            if (!_context.Actions.Any())
            {
                // Action
                await _context.Actions.AddRangeAsync(
                    new ActionEntity
                    {
                        IsEnabled = true,
                        Name = "read"
                    },
                    new ActionEntity
                    {
                        IsEnabled = true,
                        Name = "write"
                    },
                    new ActionEntity
                    {
                        IsEnabled = true,
                        Name = "delete"
                    }
                    );
                await _context.SaveChangesAsync();
            }
            if (!_context.Permissions.Any())
            {
                // Permission
                await _context.Permissions.AddRangeAsync(
                    new Permission
                    {
                        IsEnabled = true,
                        UserId = 1,
                        AlbumId = 1,
                        MangaId = 1,
                        ActionId = 1,
                        PermissionType = "Grant"
                    },
                    new Permission
                    {
                        IsEnabled = true,
                        UserId = 1,
                        AlbumId = 1,
                        MangaId = 1,
                        ActionId = 2,
                        PermissionType = "Grant"
                    },
                    new Permission
                    {
                        IsEnabled = true,
                        UserId = 1,
                        AlbumId = 1,
                        MangaId = 1,
                        ActionId = 3,
                        PermissionType = "Grant"
                    }
                    );
                await _context.SaveChangesAsync();
            }
        }
    }
}
