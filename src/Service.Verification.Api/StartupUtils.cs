using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using NSwag;

namespace Service.Verification.Api
{
    public static class StartupUtils
    {
        /// <summary>
        /// Setup swagger ui ba
        /// </summary>
        /// <param name="services"></param>
        public static void SetupSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerDocument(o =>
            {
                o.Title = "MyJetWallet API";
                o.GenerateEnumMappingDescription = true;

                o.AddSecurity("Bearer", Enumerable.Empty<string>(),
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Description = "Bearer Token",
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Name = "Authorization"
                    });
            });
        }

        /// <summary>
        /// Headers settings
        /// </summary>
        /// <param name="services"></param>
        public static void ConfigurateHeaders(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
        }
    }
}