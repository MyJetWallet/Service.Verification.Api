using System.Collections.Generic;
using Service.VerificationCodes.Grpc.Models.TokenVerification;

namespace Service.Verification.Api.Controllers.Contracts.TokenVerification
{
    public class StartVerificationResponse
    {
        public string TokenId { get; set; }
        public string VerificationId { get; set; }
        public List<AdditionalVerification> AdditionalVerifications { get; set; }
    }
}