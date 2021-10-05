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
    [Route("/api/v1/2fa")]
    public class TwoFaVerificationController : Controller
    {
        private readonly ITwoFaVerificationCodes _twoFaVerificationCodes;

        public TwoFaVerificationController(ITwoFaVerificationCodes twoFaVerificationCodes)
        {
            _twoFaVerificationCodes = twoFaVerificationCodes;
        }

        [HttpPost("request-verification")]
        public async Task<Response> Request2FaVerificationCodeAsync([FromBody] SendVerificationRequest request)
        {
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            var tokenStr = this.GetSessionToken();
            var (_, token) = MyControllerBaseHelper.ParseToken(tokenStr); 
            
            var sendRequest = new Send2FaVerificationCodeRequest()
            {
                Lang = request.Language,
                ClientId = clientId,
                Brand = this.GetBrandId(),
                RootSessionId = token.RootSessionId.ToString()
            };
            var response = await _twoFaVerificationCodes.Send2FaVerificationCodeAsync(sendRequest);
            return response.IsSuccess 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.UnsuccessfulSend);
        }
        
        [HttpPost("verify")]
        public async Task<Response> Verify2FaCodeAsync([FromBody] VerifyCodeRequest request)
        {
            var tokenStr = this.GetSessionToken();
            var (_, token) = MyControllerBaseHelper.ParseToken(tokenStr); 
            
            var verifyRequest = new Verify2FaCodeRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = request.Code,
                RootSessionId = token.RootSessionId.ToString()
            };
            var response = await _twoFaVerificationCodes.Verify2FaCodeAsync(verifyRequest);
            return response.CodeIsValid 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.InvalidCode);
        }
        
        [HttpPost("request-enable")]
        public async Task<Response> Request2FaVerificationEnableAsync([FromBody] SendVerificationRequest request)
        {
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            var sendRequest = new Send2FaChangeCodeRequest()
            {
                Lang = request.Language,
                ClientId = clientId,
                Brand = this.GetBrandId(),
                IsEnable = true
            };
            var response = await _twoFaVerificationCodes.Send2FaChangeCodeAsync(sendRequest);
            return response.IsSuccess
                ? Contracts.Response.OK()
                : response.ErrorMessage.Contains("Phone number is not confirmed")
                    ? new Response(ApiResponseCodes.PhoneIsNotConfirmed)
                    : new Response(ApiResponseCodes.UnsuccessfulSend);
        }
        
        [HttpPost("request-disable")]
        public async Task<Response> Request2FaVerificationDisableAsync([FromBody] SendVerificationRequest request)
        {
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            var sendRequest = new Send2FaChangeCodeRequest()
            {
                Lang = request.Language,
                ClientId = clientId,
                Brand = this.GetBrandId(),
                IsEnable = false
            };
            var response = await _twoFaVerificationCodes.Send2FaChangeCodeAsync(sendRequest);
            return response.IsSuccess
                ? Contracts.Response.OK()
                : response.ErrorMessage.Contains("Phone number is not confirmed")
                    ? new Response(ApiResponseCodes.PhoneIsNotConfirmed)
                    : new Response(ApiResponseCodes.UnsuccessfulSend);
        }
        
        [HttpPost("verify-enable")]
        public async Task<Response> Verify2FaEnableAsync([FromBody] VerifyCodeRequest request)
        {
            var verifyRequest = new Verify2FaChangeCodeRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = request.Code,
                IsEnable = true
            };
            var response = await _twoFaVerificationCodes.Verify2FaChangeAsync(verifyRequest);
            return response.CodeIsValid 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.InvalidCode);
        }
        
        [HttpPost("verify-disable")]
        public async Task<Response> Verify2FaDisableAsync([FromBody] VerifyCodeRequest request)
        {
            var verifyRequest = new Verify2FaChangeCodeRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = request.Code,
                IsEnable = false
            };
            var response = await _twoFaVerificationCodes.Verify2FaChangeAsync(verifyRequest);
            return response.CodeIsValid 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.InvalidCode);
        }
    }
}