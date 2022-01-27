using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Sdk.Authorization.Http;
using MyJetWallet.Sdk.WalletApi.Contracts;
using Service.Verification.Api.Controllers.Contracts;
using Service.Verification.Api.Validators;
using Service.VerificationCodes.Grpc;
using Service.VerificationCodes.Grpc.Models;
using SimpleTrading.PersonalData.Abstractions.Auth.Consts;
using ApiResponseCodes = Service.Verification.Api.Controllers.Contracts.ApiResponseCodes;
using Response = Service.Verification.Api.Controllers.Contracts.Response;
using VerifyCodeRequest = Service.Verification.Api.Controllers.Contracts.VerifyCodeRequest;

namespace Service.Verification.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/phone-setup")]
    public class PhoneSetupController : Controller
    {
        private readonly IPhoneSetupService _phoneSetupService;

        public PhoneSetupController(IPhoneSetupService phoneSetupService)
        {
            _phoneSetupService = phoneSetupService;
        }

        [HttpPost("request")]
        public async Task<Response> RequestPhoneSetupCodeAsync([FromBody] SendPhoneSetupRequest request)
        {
            var validator = new PhoneRequestValidator();
            var results = await validator.ValidateAsync(request);

            if (!results.IsValid)
                return new Response(ApiResponseCodes.InvalidPhone);
            
            if(string.IsNullOrWhiteSpace(request.Language))
                return new Response(ApiResponseCodes.LanguageNotSet);
            
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            
            var sendRequest = new SetupPhoneNumberRequest
            {
                Lang = request.Language,
                PhoneNumber = $"{request.PhoneCode}{request.PhoneBody}",
                ClientId = this.GetClientIdentity().ClientId,
                PhoneCode = request.PhoneCode,
                PhoneBody = request.PhoneBody,
                PhoneIso = request.PhoneIso,
            };
            var response = await _phoneSetupService.SetupPhoneNumberAsync(sendRequest);
            return response.IsSuccess
                ? Contracts.Response.OK()
                : response.ErrorMessage.Contains("Phone already confirmed")
                    ? new Response(ApiResponseCodes.OperationNotAllowed)
                    : new Response(ApiResponseCodes.UnsuccessfulSend);
        }
        
        [HttpPost("verify")]
        public async Task<Response> VerifyPhoneSetupAsync([FromBody] VerifyPhoneSetupRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var validator = new PhoneVerifyValidator();
            var results = await validator.ValidateAsync(request);

            if (!results.IsValid)
                return new Response(ApiResponseCodes.InvalidPhone);
            
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return new Response(ApiResponseCodes.InvalidCode);
            
            var tokenStr = this.GetSessionToken();
            var (_, token) = MyControllerBaseHelper.ParseToken(tokenStr); 
            
            var verifyRequest = new VerifyPhoneRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = request.Code,
                ClientIp = accessor.HttpContext.GetIp(),
                PhoneNumber = $"{request.PhoneCode}{request.PhoneBody}",
                RootSessionId = token.RootSessionId.ToString(),
                PhoneCode = request.PhoneCode,
                PhoneBody = request.PhoneBody,
                PhoneIso = request.PhoneIso,
            };
            var response = await _phoneSetupService.VerifyPhoneNumberAsync(verifyRequest);
            return response.CodeIsValid
                ? response.PhoneIsValid
                    ? Contracts.Response.OK()
                    : new Response(ApiResponseCodes.InvalidPhone)
                : new Response(ApiResponseCodes.InvalidCode);
        }

        [HttpGet("get-number")]
        public async Task<Contracts.Response<string>> GetPhone()
        {
            var response = await _phoneSetupService.GetUserPhoneNumber(new GetPhoneRequest()
            {
                ClientId = this.GetClientIdentity().ClientId
            });
            return response.IsSuccess 
                ? new Contracts.Response<string>(response.PhoneNumber) 
                : new Contracts.Response<string>(ApiResponseCodes.PhoneNotFound);
        }
    }
}