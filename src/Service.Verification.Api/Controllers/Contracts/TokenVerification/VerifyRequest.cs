namespace Service.Verification.Api.Controllers.Contracts.TokenVerification
{
    public class VerifyRequest
    {
        public string TokenId { get; set; }
        public string VerificationId { get; set; }
        public string Code { get; set; }
    }
}