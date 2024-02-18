using Contracts.Domains.Interfaces;
using Infrastructure.Common;
using Infrastructure.Extensions;
using Manga.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Shared.Configurations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Manga.Application.Common.Behaviours;
using MediatR;
using System.Reflection;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Application.Common.Repositories;
using Shared.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Shared.Common.Interfaces;
using Shared.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MassTransit;
using Manga.API.Application.IntegrationEvents.EventsHanler;
using System.Threading.RateLimiting;
using Manga.Application.Services.Interfaces;
using Manga.Application.Services;
using Hangfire;
using EventBus.Messages.IntegrationEvents.Interfaces;

namespace Manga.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var key = configuration["Jwt:Secret"];

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(jwt =>
            {
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                };
            }).AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Google:ClientId"];
                options.ClientSecret = configuration["Google:ClientSecret"];
            });
        }

        public static void ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(cors =>
            {
                cors.AddPolicy("AllowAll", builder =>
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            var databaseSettings = services.GetOptions<DatabaseSettings>(nameof(DatabaseSettings));
            if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.ConnectionString)
                || string.IsNullOrEmpty(databaseSettings.ConnectionHangfireString))
                throw new ArgumentNullException("Connection string is not configured.");


            services.AddDbContext<MangaContext>(options =>
            {
                options.UseSqlServer(databaseSettings.ConnectionString,
                    builder =>
                        builder.MigrationsAssembly(typeof(MangaContext).Assembly.FullName));
            });
            services.AddHangfire(option =>
            {
                option.UseSqlServerStorage(databaseSettings.ConnectionHangfireString)
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180).UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings();
            });
            services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "default", "priority" }; // Specify the queues
                options.WorkerCount = 20; // Set the maximum degree of parallelism
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

        public static void ConfigureAzureBlob(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureBlobSettings>(configuration.GetSection("AzureClient"));
        }

        public static void ConfigureMassTransit(this IServiceCollection services)
        {
            var settings = services.GetOptions<EventBusSettings>(nameof(EventBusSettings));
            if (settings == null || string.IsNullOrEmpty(settings.HostAddress) ||
                string.IsNullOrEmpty(settings.HostAddress)) throw new ArgumentNullException("EventBusSettings is not configured!");

            var mqConnection = new Uri(settings.HostAddress);

            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(mqConnection);
                });
                // Publish message, instead of sending it to a specific queue directly.
                config.AddRequestClient<IChapterCreatedEvent>();
            });
        }

        public static void ConfigureRateLimtter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy("fixed", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 4,
                                Window = TimeSpan.FromSeconds(12)
                            }));
            });
        }

        public static void ConfigureRedis(this IServiceCollection services)
        {
            var settings = services.GetOptions<RedisSettings>(nameof(RedisSettings));
            if (string.IsNullOrEmpty(settings.ConnectionString))
                throw new ArgumentNullException("Redis Connection string is not configured.");
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = settings.ConnectionString;
            });
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services
            .AddAutoMapper(Assembly.GetExecutingAssembly())
            .AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            })
            .AddHttpContextAccessor()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>)).AddScoped<MangaContextSeed>()
            .AddScoped<MangaContextSeed>()
            .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>))
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddScoped<IMangaRepository, MangaRepository>()
            .AddScoped<IMangaGenreRepository, MangaGenreRepository>()
            .AddScoped<IGenreRepository, GenreRepository>()
            .AddScoped<IChapterRepository, ChapterRepository>()
            .AddScoped<IChapterImageRepository, ChapterImageRepository>()
            .AddScoped<ISasTokenGenerator, SasTokenGenerator>()
            .AddScoped<IBaseAuthService, BaseAuthService>()
            .AddScoped<ICommentRepository, CommentRepository>()
            .AddScoped<ICommentReactionRepository, CommentReactionRepository>()
            .AddScoped<IMangaReactionRepository, MangaReactionRepository>()
            .AddScoped<IMangaInteractionService, MangaInteractionService>()
            .AddScoped<IRedisCacheRepository, RedisCacheRepository>()
            ;
    }
}
