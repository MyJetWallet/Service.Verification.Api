using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Microsoft.AspNetCore.Http;
using MyJetWallet.ApiSecurityManager.Autofac;
using MyJetWallet.Sdk.Authorization.NoSql;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.RestApiTrace;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using Service.ClientBlocker.Client;
using Service.ClientProfile.Client;

namespace Service.Verification.Api.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterEncryptionServiceClient();
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            
            VerificationCodes.Client.AutofacHelper.RegisterVerificationCodesClient(builder, Program.Settings.VerificationCodesGrpcUrl);
            
            builder.RegisterClientBlockerClient(Program.Settings.ClientBlockerGrpcClient);
            builder.RegisterClientProfileClientWithoutCache(Program.Settings.ClientProfileGrpcServiceUrl);
            
            RegisterAuthServices(builder);
            
            if (Program.Settings.EnableApiTrace)
            {
                builder
                    .RegisterInstance(new ApiTraceManager(
                        Program.Settings.ElkLogs, 
                        $"api-trace-{Program.Settings.ElkLogs.IndexPrefix}",
                        Program.LogFactory.CreateLogger("ApiTraceManager")))
                    .As<IApiTraceManager>()
                    .As<IStartable>()
                    .AutoActivate()
                    .SingleInstance();
            }
        }

        protected void RegisterAuthServices(ContainerBuilder builder)
        {
            var authNoSql = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.AuthMyNoSqlReaderHostPort));
            builder.RegisterMyNoSqlReader<ShortRootSessionNoSqlEntity>(authNoSql, ShortRootSessionNoSqlEntity.TableName);
        }
    }
}