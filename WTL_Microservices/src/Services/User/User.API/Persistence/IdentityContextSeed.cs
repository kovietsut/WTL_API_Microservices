using UserEntity = User.API.Entities.User;
using ILogger = Serilog.ILogger;
using Microsoft.EntityFrameworkCore;
using User.API.Entities;

namespace User.API.Persistence
{
    public class IdentityContextSeed
    {
        private readonly ILogger _logger;
        private readonly IdentityContext _context;

        public IdentityContextSeed(ILogger logger, IdentityContext context)
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
            if (!_context.Roles.Any())
            {
                // Role
                await _context.Roles.AddRangeAsync(
                    new Role
                    {
                        IsEnabled = true,
                        Name = "ADMIN"
                    },
                    new Role
                    {
                        IsEnabled = true,
                        Name = "AUTHOR"
                    },
                    new Role
                    {
                        IsEnabled = true,
                        Name = "USER"
                    },
                    new Role
                    {
                        IsEnabled = true,
                        Name = "TRANSLATOR"
                    },
                    new Role
                    {
                        IsEnabled = true,
                        Name = "ISSUER"
                    });
                await _context.SaveChangesAsync();
            }
            if (!_context.Users.Any())
            {
                // User
                await _context.Users.AddRangeAsync(
                    new UserEntity
                    {
                        IsEnabled = true,
                        RoleId = 1,
                        Email = "nguyentienphat9x@gmail.com",
                        PasswordHash = "nld7cvF70f2JNhIOie8Wy1/VZza04zDXmZ8BtGjFBBE=",
                        SecurityStamp = "d6a69959-ab31-4653-a52d-299e0174503e",
                        GoogleUserId = null,
                        FullName = "Phat Nguyen",
                        PhoneNumber = "0369427565",
                        Address = "Ho Chi Minh",
                        Gender = "Nam",
                        AvatarPath = null
                    },
                    new UserEntity
                    {
                        IsEnabled = true,
                        RoleId = 5,
                        Email = "issuer@gmail.com",
                        PasswordHash = "nld7cvF70f2JNhIOie8Wy1/VZza04zDXmZ8BtGjFBBE=",
                        GoogleUserId = null,
                        FullName = "Issuer",
                        PhoneNumber = "0909255458",
                        Address = "Ho Chi Minh",
                        Gender = "Nam",
                        AvatarPath = null
                    }
                    );
                await _context.SaveChangesAsync();
            }
            if (!_context.Tokens.Any())
            {
                // Token
                await _context.Tokens.AddRangeAsync(
                    new Token
                    {
                        IsEnabled = true,
                        UserId = 1,
                        AccessToken = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Im5ndXllbnRpZW5waGF0OXhAZ21haWwuY29tIiwic3ViIjoiMSIsImp0aSI6IjJkNTM4Njk4LTFiOGQtNDBiYy1hMmI1LWFkYTRiM2RmNDU4MiIsIklkIjoiMSIsIkVtYWlsIjoibmd1eWVudGllbnBoYXQ5eEBnbWFpbC5jb20iLCJSb2xlIjoiMSIsIm5iZiI6MTY5OTA4NDIyNSwiZXhwIjoxNjk5Njg5MDI1LCJpYXQiOjE2OTkwODQyMjUsImlzcyI6Iklzc3VlciIsImF1ZCI6Iklzc3VlciJ9._c3RxdDeJgva4lK6U_30XFxaE_J95BFR1RQvu098qsN_P4-18TPt4t946aR7RcH5qtxPS9jEPZc87apZ8Kbt_Q",
                        RefreshToken = "ynhtycdKmroi6XmjjmhzMxV/nl1ELjnlKnpRJD/J9xA=",
                        AccessTokenExpiration = DateTime.Parse("2023-12-31 14:50:25.553"),
                        RefreshTokenExpiration = DateTime.Parse("2023-12-31 14:50:25.553"),
                        IsRevoked = false,
                        JwtId = "2d538698-1b8d-40bc-a2b5-ada4b3df4582"
                    });
                await _context.SaveChangesAsync();
            }
        }
    }
}
