namespace Service.Verification.Api.Controllers.Contracts
{
    public class SendVerificationRequest
    {
        public string Language { get; set; }
        
        public string DeviceType { get; set; }
    }
}