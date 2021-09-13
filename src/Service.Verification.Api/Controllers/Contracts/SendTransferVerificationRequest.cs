namespace Service.Verification.Api.Controllers.Contracts
{
    public class SendTransferVerificationRequest
    {
        public string Language { get; set; }
        
        public string OperationId { get; set; }

        public string AssetSymbol { get; set; }

        public string Amount { get; set; }

        public string DestinationPhone { get; set; }
    }
}