using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Infrastructure.Common;
using Infrastructure.Extensions;
using Manga.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Configurations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Manga.Application.Common.Behaviours;
using MediatR;
using System.Net.NetworkInformation;
using System.Reflection;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Application.Common.Repositories;
using Manga.Application.Features.Mangas.Queries;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

namespace Manga.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.ConnectionString))
                throw new ArgumentNullException("Connection string is not configured.");

            services.AddDbContext<MangaContext>(options =>
            {
                options.UseSqlServer(databaseSettings.ConnectionString,
                    builder =>
                        builder.MigrationsAssembly(typeof(MangaContext).Assembly.FullName));
            });
            return services;
        }

        public static void ConfigureHealthChecks(this IServiceCollection services)
        {
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            services.AddHealthChecks()
                .AddSqlServer(databaseSettings.ConnectionString,
                    name: "SqlServer Health",
                    failureStatus: HealthStatus.Degraded);
        }

        public static void ConfigureErrorCode(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ErrorCode>(configuration.GetSection("ErrorCode"));
        }

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "Manga.API", Version = "v1" });
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
                });
                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
            });
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services
            .AddAutoMapper(Assembly.GetExecutingAssembly())
            .AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            })
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>)).AddScoped<MangaContextSeed>()
            .AddScoped<MangaContextSeed>()
            .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>))
            .AddScoped<IMangaRepository, MangaRepository>()
            ;
    }
}
