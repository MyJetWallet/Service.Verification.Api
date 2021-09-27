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
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            var sendRequest = new SetupPhoneNumberRequest
            {
                Lang = request.Language,
                PhoneNumber = request.PhoneNumber,
                ClientId = this.GetClientIdentity().ClientId,
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
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return new Response(ApiResponseCodes.InvalidCode);
            
            var verifyRequest = new VerifyPhoneRequest()
            {
                ClientId = this.GetClientIdentity().ClientId,
                Code = request.Code,
                ClientIp = accessor.HttpContext.GetIp(),
                PhoneNumber = request.PhoneNumber
            };
            var response = await _phoneSetupService.VerifyPhoneNumberAsync(verifyRequest);
            return response.CodeIsValid
                ? response.PhoneIsValid
                    ? Contracts.Response.OK()
                    : new Response(ApiResponseCodes.InvalidPhone)
                : new Response(ApiResponseCodes.InvalidCode);
        }

        [HttpGet("get-number")]
        public async Task<Response<string>> GetPhone()
        {
            var response = await _phoneSetupService.GetUserPhoneNumber(new GetPhoneRequest()
            {
                ClientId = this.GetClientIdentity().ClientId
            });
            return response.IsSuccess 
                ? new Response<string>(response.PhoneNumber) 
                : new Response<string>(ApiResponseCodes.PhoneNotFound);
        }
    }
}