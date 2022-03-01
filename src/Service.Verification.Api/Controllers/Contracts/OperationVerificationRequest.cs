namespace Service.Verification.Api.Controllers.Contracts
{
    public class OperationVerificationRequest
    {
        public string Code { get; set; }
        public string OperationId { get; set; }
        public string Brand { get; set; }
    }
}