using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Verification.Api.Controllers.Contracts;
using Service.VerificationCodes.Grpc;
using Service.VerificationCodes.Grpc.Models;
using VerifyCodeRequest = Service.Verification.Api.Controllers.Contracts.VerifyCodeRequest;

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
        public async Task<Response<string>> RequestEmailVerificationCodeAsync([FromBody] SendVerificationRequest request)
        {
            var sendRequest = new SendVerificationCodeRequest
            {
                Lang = request.Language,
                ClientId = this.GetClientIdentity().ClientId
            };
            var response = await _emailVerificationService.SendEmailVerificationCodeAsync(sendRequest);
            return response.IsSuccess 
                ? new Response<string>(ApiResponseCodes.OK)
                : new Response<string>(ApiResponseCodes.InvalidCode)
                {
                    Data = response.ErrorMessage
                };
            
        }
        
        [HttpPost("verify")]
        public async Task<Response> VerifyEmailCodeAsync([FromBody] VerifyCodeRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var verifyRequest = new VerificationCodes.Grpc.Models.VerifyCodeRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = request.Code,
                ClientIp = accessor.HttpContext.GetIp()
            };
            var response = await _emailVerificationService.VerifyEmailCodeAsync(verifyRequest);
            return response.CodeIsValid 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.InvalidCode);
        }
    }
}