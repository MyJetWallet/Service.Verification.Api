namespace Service.Verification.Api.Controllers.Contracts
{
    public class SendWithdrawalVerificationRequest
    {
        public string Language { get; set; }
        
        public string DeviceType { get; set; }
        
        public string OperationId { get; set; }

        public string AssetSymbol { get; set; }

        public string Amount { get; set; }

        public string DestinationAddress { get; set; }
        
    }
}