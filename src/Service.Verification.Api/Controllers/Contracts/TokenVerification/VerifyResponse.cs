using System.Collections.Generic;
using Service.VerificationCodes.Grpc.Models.TokenVerification;

namespace Service.Verification.Api.Controllers.Contracts.TokenVerification
{
    public class VerifyResponse
    {
        public bool VerificationValid { get; set; }
        public bool TokenValid { get; set; }
        public List<AdditionalVerification> AdditionalVerifications { get; set; }
    }
}