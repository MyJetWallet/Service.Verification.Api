namespace Service.Verification.Api.Controllers.Contracts
{
    public class TokenRequest
    {
        public string Token { get; set; }
        public string OperationId { get; set; }
    }
}