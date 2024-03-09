using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shared.Common.Interfaces;
using Shared.Common;
using Shared.DTOs;
using System.Text;
using Hangfire.API.Repositories.Interfaces;
using Hangfire.API.Repositories;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Configurations;
using Hangfire.API.Application.IntegrationEvent.EventHandler;
using Infrastructure.Extensions;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Contracts.Services;
using Infrastructure.Exceptions;

namespace Hangfire.API.Extensions
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
            }).AddCookie();
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
                swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "Hangfire.API", Version = "v1" });
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

        public static void ConfigureErrorCode(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ErrorCode>(configuration.GetSection("ErrorCode"));
        }

        public static IServiceCollection AddConfigurationSettings(this IServiceCollection services,
        IConfiguration configuration)
        {
            var hangFireSettings = configuration.GetSection(nameof(HangFireSettings))
                .Get<HangFireSettings>();
            services.AddSingleton(hangFireSettings);

            var emailSettings = configuration.GetSection(nameof(SMTPEmailSetting))
                .Get<SMTPEmailSetting>();
            services.AddSingleton(emailSettings);
            return services;
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
                config.AddConsumersFromNamespaceContaining<EmailNotificationEventHandler>();
                config.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(mqConnection);
                    //cfg.ReceiveEndpoint("created-email-notifications-queue", c =>
                    //{
                    //    c.ConfigureConsumer<EmailNotificationEventHandler>(ctx);
                    //});
                    cfg.ConfigureEndpoints(ctx);
                });
                // Publisher
                //config.AddRequestClient<IChapterCreatedEvent>();
            });
        }

        //public static void ConfigureRateLimtter(this IServiceCollection services)
        //{
        //    services.AddRateLimiter(options =>
        //    {
        //        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        //        options.AddPolicy("fixed", httpContext =>
        //            RateLimitPartition.GetFixedWindowLimiter(
        //                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
        //                    factory: _ => new FixedWindowRateLimiterOptions
        //                    {
        //                        PermitLimit = 4,
        //                        Window = TimeSpan.FromSeconds(12)
        //                    }));
        //    });
        //}

        public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
            services.AddTransient<IScheduledJobService, HangfireService>()
            .AddScoped<ISmtpEmailService, SmtpEmailService>()
            .AddScoped<IBackgroundJobRepository, BackgroundJobRepository>()
            ;
    }
}
