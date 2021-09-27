using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Verification.Api.Controllers.Contracts;
using Service.VerificationCodes.Grpc;
using Service.VerificationCodes.Grpc.Models;
using SimpleTrading.PersonalData.Abstractions.Auth.Consts;
using VerifyCodeRequest = Service.Verification.Api.Controllers.Contracts.VerifyCodeRequest;

namespace Service.Verification.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/sms-verification")]
    public class SmsVerificationController : Controller
    {
        private readonly ISmsVerificationCodes _smsVerificationService;

        public SmsVerificationController(ISmsVerificationCodes smsVerificationService)
        {
            _smsVerificationService = smsVerificationService;
        }

        [HttpPost("request")]
        public async Task<Response> RequestSmsVerificationCodeAsync([FromBody] SendVerificationRequest request)
        {
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            var sendRequest = new SendVerificationCodeRequest
            {
                Lang = request.Language,
                ClientId = this.GetClientIdentity().ClientId
            };
            var response = await _smsVerificationService.SendSmsVerificationCodeAsync(sendRequest);
            return response.IsSuccess
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.UnsuccessfulSend);
        }
        
        [HttpPost("verify")]
        public async Task<Response> VerifySmsCodeAsync([FromBody] VerifyCodeRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var verifyRequest = new VerificationCodes.Grpc.Models.VerifyCodeRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = request.Code,
                ClientIp = accessor.HttpContext.GetIp()
            };
            var response = await _smsVerificationService.VerifySmsCodeAsync(verifyRequest);
            return response.CodeIsValid 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.InvalidCode);
        }
    }
}