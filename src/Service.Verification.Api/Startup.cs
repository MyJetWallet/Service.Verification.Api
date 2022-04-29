using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Autofac;
using MyJetWallet.Sdk.Authorization.Http;
using MyJetWallet.Sdk.GrpcMetrics;
using MyJetWallet.Sdk.GrpcSchema;
using MyJetWallet.Sdk.Service;
using Prometheus;
using ProtoBuf.Grpc.Server;
using Service.Verification.Api.Middleware;
using Service.Verification.Api.Modules;
using SimpleTrading.BaseMetrics;
using SimpleTrading.ServiceStatusReporterConnector;
using SimpleTrading.TokensManager;
using MyJetWallet.ApiSecurityManager.Autofac;
using MyJetWallet.Sdk.WalletApi;

namespace Service.Verification.Api
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            StartupUtils.SetupWalletServices(services, Program.Settings.SessionEncryptionApiKeyId);

            services.AddHttpContextAccessor();
            services.ConfigureJetWallet<ApplicationLifetimeManager>(Program.Settings.ZipkinUrl);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            StartupUtils.SetupWalletApplication(app, env, Program.Settings.EnableApiTrace, "api");
            app.UseEndpoints(endpoints =>
            {
                //security
                endpoints.RegisterGrpcServices();
                endpoints.MapControllers();
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.ConfigureJetWallet();
            builder.RegisterModule<SettingsModule>();
            builder.RegisterModule<ServiceModule>();
        }
    }
}
