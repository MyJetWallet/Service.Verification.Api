namespace Service.Verification.Api.Controllers.Contracts
{
    public class VerifyPhoneSetupRequest
    {
        public string Code { get; set; }
        public string PhoneNumber { get; set; }
    }
}