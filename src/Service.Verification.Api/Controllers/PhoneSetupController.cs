using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.ApiSecurityManager.ApiKeys;
using MyJetWallet.Sdk.Authorization.Extensions;
using MyJetWallet.Sdk.Authorization.Http;
using MyJetWallet.Sdk.WalletApi.Contracts;
using Service.ClientBlocker.Grpc;
using Service.ClientBlocker.Grpc.Models;
using Service.ClientProfile.Domain.Models;
using Service.ClientProfile.Grpc;
using Service.ClientProfile.Grpc.Models.Requests;
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
        private readonly IApiKeyStorage _apiKeyStorage;

        private readonly IClientProfileService _clientProfile;
        private readonly IClientAttemptService _attemptService;

        public PhoneSetupController(IPhoneSetupService phoneSetupService,
            IApiKeyStorage apiKeyStorage, IClientProfileService clientProfile, IClientAttemptService attemptService)
        {
            _phoneSetupService = phoneSetupService;
            _apiKeyStorage = apiKeyStorage;
            _clientProfile = clientProfile;
            _attemptService = attemptService;
        }

        [HttpPost("request")]
        public async Task<Response> RequestPhoneSetupCodeAsync([FromBody] SendPhoneSetupRequest request)
        {
            if (request.PhoneCode != null && !request.PhoneCode.StartsWith('+'))
                request.PhoneCode = $"+{request.PhoneCode}";
            
            var validator = new PhoneRequestValidator();
            var results = await validator.ValidateAsync(request);

            if (!results.IsValid)
                return new Response(ApiResponseCodes.InvalidPhone);
            
            if(string.IsNullOrWhiteSpace(request.Language))
                return new Response(ApiResponseCodes.LanguageNotSet);
            
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            var clientProfile = await _clientProfile.GetOrCreateProfile(new GetClientProfileRequest
            {
                ClientId = clientId
            });
            
            var clientBlockers = clientProfile.Blockers?
                .FirstOrDefault(itm =>
                    itm.BlockedOperationType == BlockingType.PhoneNumberUpdate && DateTime.UtcNow < itm.ExpiryTime);

            if (clientBlockers != null)
            {
                throw new WalletApiErrorBlockerException("Cant request verification, found blocker", clientBlockers.ExpiryTime - DateTime.UtcNow);
            }
            
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
            if (request.PhoneCode != null && !request.PhoneCode.StartsWith('+'))
                request.PhoneCode = $"+{request.PhoneCode}";
            
            var validator = new PhoneVerifyValidator();
            var results = await validator.ValidateAsync(request);

            if (!results.IsValid)
                return new Response(ApiResponseCodes.InvalidPhone);
            
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return new Response(ApiResponseCodes.InvalidCode);
            
            var tokenStr = this.GetSessionToken();
            var (_, token) = _apiKeyStorage.ParseToken(Program.Settings.SessionEncryptionApiKeyId, tokenStr);
            
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

            if (!response.CodeIsValid)
            {
                await _attemptService.TrackPhoneNumberAttempt(new TrackAttemptRequest
                {
                    ClientId = clientId,
                    IsSuccess = false
                });
                return new Response(ApiResponseCodes.InvalidCode);
            }
            
            if (!response.PhoneIsValid)
            {
                await _attemptService.TrackPhoneNumberAttempt(new TrackAttemptRequest
                {
                    ClientId = clientId,
                    IsSuccess = false
                });
                return new Response(ApiResponseCodes.InvalidPhone);
            }

            if (response.PhoneIsDuplicate)
            {
                await _attemptService.TrackPhoneNumberAttempt(new TrackAttemptRequest
                {
                    ClientId = clientId,
                    IsSuccess = false
                });
                return new Response(ApiResponseCodes.PhoneDuplicate);
            }
            
            await _attemptService.TrackPhoneNumberAttempt(new TrackAttemptRequest
            {
                ClientId = clientId,
                IsSuccess = true
            });
            return new Response(ApiResponseCodes.OK);
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