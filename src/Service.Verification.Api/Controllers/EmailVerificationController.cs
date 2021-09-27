using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Sdk.Authorization.Http;
using Service.Verification.Api.Controllers.Contracts;
using Service.VerificationCodes.Grpc;
using Service.VerificationCodes.Grpc.Models;
using SimpleTrading.PersonalData.Abstractions.Auth.Consts;
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
        public async Task<Response> RequestEmailVerificationCodeAsync([FromBody] SendVerificationRequest request)
        {
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            var sendRequest = new SendVerificationCodeRequest
            {
                Lang = request.Language,
                ClientId = clientId,
                Brand = this.GetBrandId(),
                DeviceType = request.DeviceType
            };
            var response = await _emailVerificationService.SendEmailVerificationCodeAsync(sendRequest);
            return response.IsSuccess 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.UnsuccessfulSend);
        }
        
        [HttpPost("verify")]
        public async Task<Response> VerifyEmailCodeAsync([FromBody] VerifyCodeRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return new Response(ApiResponseCodes.InvalidCode);
            
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