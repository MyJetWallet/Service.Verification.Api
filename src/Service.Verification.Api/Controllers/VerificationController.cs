using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Verification.Api.Controllers.Contracts;
using Service.Verification.Api.Controllers.Contracts.TokenVerification;
using Service.VerificationCodes.Grpc;
using Service.VerificationCodes.Grpc.Models.TokenVerification;
using SimpleTrading.PersonalData.Abstractions.Auth.Consts;
using ApiResponseCodes = Service.Verification.Api.Controllers.Contracts.ApiResponseCodes;

namespace Service.Verification.Api.Controllers
{
    
    [Authorize]
    [ApiController]
    [Route("/api/v1/verification")]
    public class VerificationController : Controller
    {
        private readonly IVerificationService _verificationService;

        public VerificationController(IVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        [HttpPost("request")]
        public async Task<Response<StartVerificationResponse>> RequestVerificationAsync([FromBody] StartVerificationRequest request,  [FromServices] IHttpContextAccessor accessor)
        {            
            if(string.IsNullOrWhiteSpace(request.Language))
                return new Response<StartVerificationResponse>(ApiResponseCodes.LanguageNotSet);
            
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return new Response<StartVerificationResponse>(new StartVerificationResponse()
                {
                    TokenId = string.IsNullOrEmpty(request.TokenId) ? Guid.NewGuid().ToString("N") : request.TokenId,
                    VerificationId = string.IsNullOrEmpty(request.VerificationId) ? Guid.NewGuid().ToString("N") : request.VerificationId,
                    AdditionalVerifications = null
                });

            var response = await _verificationService.RequestVerification(new VerificationRequest
            {
                ClientId = clientId,
                VerificationId = request.VerificationId,
                TokenId = request.TokenId,
                VerificationReason = request.Reason,
                Type = request.Type,
                Language = request.Language,
                DeviceType = request.DeviceType,
                Platform = request.Platform
            });

            return new Response<StartVerificationResponse>(
                new StartVerificationResponse
                {
                    TokenId = response.TokenId,
                    VerificationId = response.VerificationId,
                    AdditionalVerifications = response.AdditionalVerifications
                });
        }
        
        [HttpPost("verify")]
        public async Task<Response<VerifyResponse>> VerifyTokenAsync([FromBody] VerifyRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return new Response<VerifyResponse>(ApiResponseCodes.InvalidCode);

            var response = await _verificationService.VerifyCode(new VerificationAttemptRequest
            {
                ClientId = clientId,
                TokenId = request.TokenId,
                VerificationId = request.VerificationId,
                Code = request.Code
            });
            
            if(!response.VerificationValid)
                return new Response<VerifyResponse>(ApiResponseCodes.InvalidCode);
            
            return new Response<VerifyResponse>(new VerifyResponse
            {
                VerificationValid = response.VerificationValid,
                TokenValid = response.TokenValid,
                AdditionalVerifications = response.AdditionalVerifications
            });
        }
        
    }
}