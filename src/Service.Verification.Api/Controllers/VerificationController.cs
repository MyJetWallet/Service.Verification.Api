using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.VerificationCodes.Grpc;
using Service.VerificationCodes.Grpc.Models;

namespace Service.Verification.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/verification/email")]
    public class VerificationController : Controller
    {
        private readonly IEmailVerificationCodes _emailVerificationService;

        public VerificationController(IEmailVerificationCodes emailVerificationService)
        {
            _emailVerificationService = emailVerificationService;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestEmailVerificationCodeAsync([FromBody] string lang)
        {
            var request = new SendVerificationCodeRequest()
            {
                Lang = lang,
                ClientId = this.GetClientIdentity().ClientId,
            };
            var response = await _emailVerificationService.SendEmailVerificationCodeAsync(request);
            return Ok(response);
        }
        
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyEmailCodeAsync([FromBody] string code, [FromServices] IHttpContextAccessor accessor)
        {

            var request = new VerifyCodeRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = code,
                ClientIp = accessor.HttpContext.GetIp()
            };
            var response = await _emailVerificationService.VerifyEmailCodeAsync(request);
            return Ok(response);
        }
    }
}