using Common.Logging;
using Contracts.Domains.Interfaces;
using Infrastructure.Common;
using Infrastructure.Common.Repositories;
using Infrastructure.Extensions;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Configurations;
using Shared.DTOs;
using System.Text;
using User.API.Application.IntegrationEvent.EventHandler;
using User.API.HttpRepository;
using User.API.HttpRepository.Interfaces;
using User.API.Persistence;
using User.API.Repositories;
using User.API.Repositories.Interfaces;

namespace User.API.Extensions
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

        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "User.API", Version = "v1" });
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

        public static void ConfigureErrorCode(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ErrorCode>(configuration.GetSection("ErrorCode"));
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

        public static void ConfigureMassTransit(this IServiceCollection services)
        {
            var settings = services.GetOptions<EventBusSettings>(nameof(EventBusSettings));
            if (settings == null || string.IsNullOrEmpty(settings.HostAddress) ||
                string.IsNullOrEmpty(settings.HostAddress)) throw new ArgumentNullException("EventBusSettings is not configured!");

            var mqConnection = new Uri(settings.HostAddress);

            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(config =>
            {
                config.AddConsumersFromNamespaceContaining<ChapterCreatedEventHandler>();
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(mqConnection);
                    //cfg.ReceiveEndpoint("created-chapter-queue", c =>
                    //{
                    //    c.ConfigureConsumer<ChapterCreatedEventHandler>(ctx);
                    //});
                    cfg.ConfigureEndpoints(ctx);
                });
                // Publisher
                //config.AddRequestClient<IChapterCreatedEvent>();
            });
        }

        public static void ConfigureHttpClients(this IServiceCollection services)
        {
            ConfigureElasticSearchUserHttpClient(services);
            ConfigureAuthenticateHttpClient(services);
        }

        private static void ConfigureElasticSearchUserHttpClient(this IServiceCollection services)
        {   
            var urls = services.GetOptions<ServiceUrls>(nameof(ServiceUrls));
            if (urls == null || string.IsNullOrEmpty(urls.ElasticSearch))
                throw new ArgumentNullException("ServiceUrls ElasticSearch is not configured");

            services.AddHttpClient<IElasticSearchUserHttpRepository, ElasticSearchUserHttpRepository>("ElasticSearchAPI", (sp, cl) =>
            {
                cl.BaseAddress = new Uri($"{urls.ElasticSearch}/api/");
            }).AddHttpMessageHandler<LoggingDelegatingHandler>();
            services.AddScoped(sp => sp.GetService<IHttpClientFactory>()
                .CreateClient("ElasticSearchAPI"));
        }

        private static void ConfigureAuthenticateHttpClient(this IServiceCollection services)
        {
            var urls = services.GetOptions<ServiceUrls>(nameof(ServiceUrls));
            if (urls == null || string.IsNullOrEmpty(urls.Authenticate))
                throw new ArgumentNullException("ServiceUrls Authenticate is not configured");

            services.AddHttpClient<IAuthenticateHttpRepository, AuthenticateHttpRepository>("AuthenticateAPI", (sp, cl) =>
            {
                cl.BaseAddress = new Uri($"{urls.Authenticate}/api/");
            }).AddHttpMessageHandler<LoggingDelegatingHandler>();
            services.AddScoped(sp => sp.GetService<IHttpClientFactory>()
                .CreateClient("ElasticSearchAPI"));
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services.AddScoped<IdentityContextSeed>()
            .AddScoped(typeof(IRepositoryBase<,,>), typeof(RepositoryBase<,,>))
            .AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>))
            .AddScoped<IEncryptionRepository, EncryptionRepository>()
            .AddScoped<IAuthenticationRepository, AuthenticationRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<ITokenRepository, TokenRepository>()
            .AddScoped<IRedisCacheRepository, RedisCacheRepository>()
            .AddTransient<LoggingDelegatingHandler>()
            ;
    }
}