using Hangfire;
using HangfireBasicAuthenticationFilter;
using HealthChecks.UI.Client;
using Infrastructure.Middlewares;
using Manga.Application.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Manga.API.Extensions
{
    public static class ApplicationExtensions
    {
        public static void UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Manga API");
                c.DisplayRequestDuration();
            });
            app.UseMiddleware<JWTMiddleware>();
            app.UseMiddleware<ErrorWrappingMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowAll");
            app.UseRateLimiter();
            // app.UseHttpsRedirection(); //for production only
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
