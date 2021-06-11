using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Verification.Api.Controllers.Contracts;
using Service.VerificationCodes.Grpc;
using Service.VerificationCodes.Grpc.Models;

namespace Service.Verification.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/verification/email")]
    public class EmailVerificationController : Controller
    {
        private readonly IEmailVerificationCodes _emailVerificationService;

        public EmailVerificationController(IEmailVerificationCodes emailVerificationService)
        {
            _emailVerificationService = emailVerificationService;
        }

        [HttpPost("request")]
        public async Task<Response> RequestEmailVerificationCodeAsync([FromBody] string lang)
        {
            var request = new SendVerificationCodeRequest
            {
                Lang = lang,
                ClientId = this.GetClientIdentity().ClientId
            };
            var response = await _emailVerificationService.SendEmailVerificationCodeAsync(request);
            return response.IsSuccess
                ? Contracts.Response.OK()
                : throw new VerificationApiErrorException(response.ErrorMessage, ApiResponseCodes.UnsuccessfulSend);
        }
        
        [HttpPost("verify")]
        public async Task<Response> VerifyEmailCodeAsync([FromBody] string code, [FromServices] IHttpContextAccessor accessor)
        {
            var request = new VerifyCodeRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = code,
                ClientIp = accessor.HttpContext.GetIp()
            };
            var response = await _emailVerificationService.VerifyEmailCodeAsync(request);
            return response.CodeIsValid 
                ? Contracts.Response.OK() 
                : throw new VerificationApiErrorException("Invalid verification code", ApiResponseCodes.InvalidCode);
        }
    }
}