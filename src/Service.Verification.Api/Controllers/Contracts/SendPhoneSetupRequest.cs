namespace Service.Verification.Api.Controllers.Contracts
{
    public class SendPhoneSetupRequest
    {
        public string Language { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneBody { get; set; }
        public string PhoneIso { get; set; }

    }
}