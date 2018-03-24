using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using TomKerkhove.Demos.KeyVault.API.Providers;
using TomKerkhove.Demos.KeyVault.API.Providers.Interfaces;

namespace TomKerkhove.Demos.KeyVault.API
{
    public class Startup
    {
        private const string OpenApiTitle = "Azure Key Vault Samples";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        public static bool IsDevelopment { get; private set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            IsDevelopment = env.IsDevelopment();

            app.UseMvc();
            UseOpenApiUi(app);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<ITelemetryProvider, ApplicationInsightsTelemetryProvider>();
            services.AddSingleton<ISecretProvider, SecretProvider>();
            services.AddSingleton<ICachedSecretProvider, MemoryCachedSecretProvider>();
            ConfigureOpenApiSpecificationGeneration(services);
        }

        private static void ConfigureOpenApiSpecificationGeneration(IServiceCollection services)
        {
            var openApiInformation = new Info
            {
                Contact = new Contact
                {
                    Name = "Tom Kerkhove"
                },
                Title = $"{OpenApiTitle} v1",
                Description = "Collection of samples how you can use Azure Key Vault",
                Version = "v1"
            };

            services.AddSwaggerGen(swaggerGenerationOptions =>
            {
                swaggerGenerationOptions.SwaggerDoc("v1", openApiInformation);
                swaggerGenerationOptions.DescribeAllEnumsAsStrings();
                swaggerGenerationOptions.TagActionsBy(apiDescription =>
                {
                    var routeAttribute = (RouteAttribute) apiDescription.ControllerAttributes().Single(attribute => attribute.GetType() == typeof(RouteAttribute));
                    return routeAttribute.Name;
                });
            });
        }

        private static void UseOpenApiUi(IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(swaggerUiOptions =>
            {
                swaggerUiOptions.SwaggerEndpoint("/swagger/v1/swagger.json", OpenApiTitle);
                swaggerUiOptions.DisplayOperationId();
                swaggerUiOptions.EnableDeepLinking();
                swaggerUiOptions.DocumentTitle = OpenApiTitle;
                swaggerUiOptions.DocExpansion(DocExpansion.List);
                swaggerUiOptions.DisplayRequestDuration();
                swaggerUiOptions.EnableFilter();
            });
        }
    }
}