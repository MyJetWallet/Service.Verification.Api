using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.Authorization.Http;

namespace Service.Verification.Api.Controllers
{
    [ApiController]
    [Route("/api/Debug")]
    public class DebugController : ControllerBase
    {
        [HttpGet("who")]
        [Authorize()]
        public IActionResult Who()
        {
            var clientId = this.GetClientId();
            var brokerId = this.GetBrokerId();
            var brandId = this.GetBrandId();
            return Ok(new JetClientIdentity(brokerId, brandId, clientId));
        }
    }
}
