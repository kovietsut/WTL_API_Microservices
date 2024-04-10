using Hangfire;
using HangfireBasicAuthenticationFilter;
using Manga.API.Extensions;
using Manga.Application.Services.Interfaces;
using Manga.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Information($"Start {builder.Environment.ApplicationName} up");
try
{
    builder.Host.AddAppConfigurations();
    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddControllersWithViews()
        .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
    builder.Services.AddApplicationServices();
    builder.Services.ConfigureHealthChecks();
    builder.Services.ConfigureCors();
    builder.Services.ConfigureSwagger();
    builder.Services.ConfigureRedis();
    builder.Services.ConfigureMassTransit();
    builder.Services.ConfigureAzureBlob(builder.Configuration);
    //builder.Services.ConfigureJWT(builder.Configuration);
    builder.Services.ConfigureErrorCode(builder.Configuration);
    //builder.Services.ConfigureMassTransit();
    builder.Services.AddInfrastructure();
    //builder.Services.ConfigureRateLimtter();
    var app = builder.Build();
    // Jobs
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[]
                {
                    new HangfireCustomBasicAuthenticationFilter
                    {
                        User = app.Configuration.GetSection("HangfireOptions:User").Value,
                        Pass = app.Configuration.GetSection("HangfireOptions:Pass").Value
                    }
                }
    });
    RecurringJob.AddOrUpdate<IMangaInteractionService>("StoreMangaInteractionToDB", x => x.StoreMangaFavoriteToDB(), Cron.Hourly);
    RecurringJob.AddOrUpdate<IMangaInteractionService>("StoreChapterFavoriteToDB", x => x.StoreChapterFavoriteToDB(), Cron.Hourly);
    app.UseInfrastructure();
    using (var scope = app.Services.CreateScope())
    {
        var contextSeed = scope.ServiceProvider.GetRequiredService<MangaContextSeed>();
        await contextSeed.InitialiseAsync();
        await contextSeed.SeedAsync();
    }
    app.Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal)) throw;

    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information($"Shut down {builder.Environment.ApplicationName} complete");
    Log.CloseAndFlush();
}