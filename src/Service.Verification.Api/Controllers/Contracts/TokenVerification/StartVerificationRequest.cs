using MyJetWallet.Domain;
using Service.VerificationCodes.Domain.Models;

namespace Service.Verification.Api.Controllers.Contracts.TokenVerification
{
    public class StartVerificationRequest
    {
        public string TokenId { get; set; }
        public string VerificationId { get; set; }
        public VerificationReason Reason { get; set; }
        public VerificationType Type { get; set; }
        public string Language { get; set; }
        public string DeviceType { get; set; }

        /// <summary>
        ///     Platform type (mobile app / web app)
        /// </summary>
        public PlatformType Platform { get; set; } = PlatformType.Spot;
    }
}