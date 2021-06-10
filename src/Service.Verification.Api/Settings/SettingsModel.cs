﻿using MyJetWallet.Sdk.Service;
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

        [YamlProperty("WalletApi.AuthMyNoSqlReaderHostPort")]
        public string AuthMyNoSqlReaderHostPort { get; set; }
    }
}
