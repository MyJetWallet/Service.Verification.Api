using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using MyJetWallet.Sdk.Authorization.NoSql;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;

namespace Service.Verification.Api.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {

            VerificationCodes.Client.AutofacHelper.RegisterVerificationCodesClient(builder, Program.Settings.VerificationCodesGrpcUrl);

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