using MyJetWallet.Domain;

namespace Service.Verification.Api.Controllers.Contracts
{
	public class SendVerificationRequest
	{
		public string Language { get; set; }

		public string DeviceType { get; set; }

		/// <summary>
		///     Platform type (mobile app / web app)
		/// </summary>
		public PlatformType Platform { get; set; } = PlatformType.Spot;
	}
}