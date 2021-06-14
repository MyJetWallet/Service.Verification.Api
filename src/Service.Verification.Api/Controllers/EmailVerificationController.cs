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
    [Route("/api/v1/email-verification")]
    public class EmailVerificationController : Controller
    {
        private readonly IEmailVerificationCodes _emailVerificationService;

        public EmailVerificationController(IEmailVerificationCodes emailVerificationService)
        {
            _emailVerificationService = emailVerificationService;
        }

        [HttpPost("request")]
        public async Task<Response> RequestEmailVerificationCodeAsync([FromBody] SendVerificationEmailRequest request)
        {
            var sendRequest = new SendVerificationCodeRequest
            {
                Lang = request.Language,
                ClientId = this.GetClientIdentity().ClientId
            };
            var response = await _emailVerificationService.SendEmailVerificationCodeAsync(sendRequest);
            return response.IsSuccess
                ? Contracts.Response.OK()
                : throw new VerificationApiErrorException(response.ErrorMessage, ApiResponseCodes.UnsuccessfulSend);
        }
        
        [HttpPost("verify")]
        public async Task<Response> VerifyEmailCodeAsync([FromBody] VerifyEmailCodeRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var verifyRequest = new VerifyCodeRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = request.Code,
                ClientIp = accessor.HttpContext.GetIp()
            };
            var response = await _emailVerificationService.VerifyEmailCodeAsync(verifyRequest);
            return response.CodeIsValid 
                ? Contracts.Response.OK() 
                : throw new VerificationApiErrorException("Invalid verification code", ApiResponseCodes.InvalidCode);
        }
    }
}