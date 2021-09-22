using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Verification.Api.Settings
{
    public class SettingsModel
    {
        [YamlProperty("VerificationApi.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("VerificationApi.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("VerificationApi.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }

        [YamlProperty("VerificationApi.AuthMyNoSqlReaderHostPort")]
        public string AuthMyNoSqlReaderHostPort { get; set; }

        [YamlProperty("VerificationApi.VerificationCodesGrpcUrl")]
        public string VerificationCodesGrpcUrl { get; set; }
        
        [YamlProperty("VerificationApi.ClientProfileGrpcServiceUrl")]
        public string ClientProfileGrpcServiceUrl { get; set; }
        
        [YamlProperty("VerificationApi.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }
    }
}
