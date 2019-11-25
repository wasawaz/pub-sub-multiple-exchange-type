using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace RabbitMqPubSub.Commons.Swaggers
{
    public static class Extensions
    {
        private const string ApiTitle = "Devops Api";
        private const string ApiVersion = "v1";

        public static void AddAppSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = ApiTitle, 
                    Version = ApiVersion
                });
            });

        }

        public static void UseAppSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "docs/{documentName}/swagger.json";
            });
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "docs";
                options.DocumentTitle = ApiTitle;
                options.SwaggerEndpoint("v1/swagger.json", ApiTitle);
                options.DefaultModelsExpandDepth(-1);
            });

        }
    }
}