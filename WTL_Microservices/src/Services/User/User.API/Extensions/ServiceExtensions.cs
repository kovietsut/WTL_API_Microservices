using Contracts.Domains.Interfaces;
using Infrastructure.Common;
using Infrastructure.Common.Repositories;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Shared.Configurations;
using User.API.Persistence;
using User.API.Repositories;
using User.API.Repositories.Interfaces;

namespace User.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureHealthChecks(this IServiceCollection services)
        {
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            services.AddHealthChecks()
                .AddSqlServer(databaseSettings.ConnectionString,
                    name: "SqlServer Health",
                    failureStatus: HealthStatus.Degraded);
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.ConnectionString))
                throw new ArgumentNullException("Connection string is not configured.");

            services.AddDbContext<IdentityContext>(options =>
            {
                options.UseSqlServer(databaseSettings.ConnectionString,
                    builder =>
                        builder.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
            });
            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services.AddScoped<IdentityContextSeed>()
            .AddScoped(typeof(IRepositoryBase<,,>), typeof(RepositoryBase<,,>))
            .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>))
            .AddScoped<IUserRepository, UserRepository>();
    }
}