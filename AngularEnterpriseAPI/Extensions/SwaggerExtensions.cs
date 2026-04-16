
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AngularEnterpriseAPI.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            // Keep swagger registration minimal to avoid direct dependency on OpenApi model types here.
            services.AddSwaggerGen();
            return services;
        }

        public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Angular Enterprise API v1");
                c.RoutePrefix = "swagger";
            });

            return app;
        }
    }
}