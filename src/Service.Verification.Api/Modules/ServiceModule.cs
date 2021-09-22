using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Microsoft.AspNetCore.Http;
using MyJetWallet.Sdk.Authorization.NoSql;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using Service.ClientProfile.Client;

namespace Service.Verification.Api.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            
            VerificationCodes.Client.AutofacHelper.RegisterVerificationCodesClient(builder, Program.Settings.VerificationCodesGrpcUrl);
            
            var noSqlClient = builder.CreateNoSqlClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort));
            builder.RegisterClientProfileClients(noSqlClient, Program.Settings.ClientProfileGrpcServiceUrl);
            
            RegisterAuthServices(builder);
        }

        protected void RegisterAuthServices(ContainerBuilder builder)
        {
            // he we do not use CreateNoSqlClient beacuse we have a problem with start many mynosql instances 
            var authNoSql = new MyNoSqlTcpClient(
                Program.ReloadedSettings(e => e.AuthMyNoSqlReaderHostPort),
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder.RegisterMyNoSqlReader<ShortRootSessionNoSqlEntity>(authNoSql, ShortRootSessionNoSqlEntity.TableName);

            authNoSql.Start();
        }
    }
}