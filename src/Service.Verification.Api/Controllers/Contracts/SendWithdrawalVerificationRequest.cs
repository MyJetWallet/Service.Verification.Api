namespace Service.Verification.Api.Controllers.Contracts
{
    public class SendWithdrawalVerificationRequest
    {
        public string Language { get; set; }
        
        public string OperationId { get; set; }

        public string AssetSymbol { get; set; }

        public string Amount { get; set; }

        public string DestinationAddress { get; set; }
        
        public string FeeAssetSymbol { get; set; }

        public string FeeAmount { get; set; }
        
    }
}