using Infrastructure.Middlewares;

namespace Hangfire.API.Extensions
{
    public static class ApplicationExtensions
    {
        public static void UseInfrastructure(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hangfire API");
                c.DisplayRequestDuration();
            });
            app.UseMiddleware<ErrorWrappingMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors("AllowAll");
            //app.UseRateLimiter();
            // app.UseHttpsRedirection(); //for production only
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
