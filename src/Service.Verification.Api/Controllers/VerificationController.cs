using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.Authorization.Http;
using Service.Verification.Api.Controllers.Contracts;
using Service.VerificationCodes.Domain.Models;
using Service.VerificationCodes.Grpc;
using Service.VerificationCodes.Grpc.Models;
using SimpleTrading.PersonalData.Abstractions.Auth.Consts;
using ApiResponseCodes = Service.Verification.Api.Controllers.Contracts.ApiResponseCodes;
using Response = Service.Verification.Api.Controllers.Contracts.Response;
using VerifyCodeRequest = Service.Verification.Api.Controllers.Contracts.VerifyCodeRequest;

namespace Service.Verification.Api.Controllers
{
    public class StartVerificationRequest
    {
        public string TokenId { get; set; }
        public string VerificationId { get; set; }
        public VerificationReason Reason { get; set; }
        public VerificationType Type { get; set; }
        public string Language { get; set; }
        public string DeviceType { get; set; }

        /// <summary>
        ///     Platform type (mobile app / web app)
        /// </summary>
        public PlatformType Platform { get; set; } = PlatformType.Spot;
    }
    
    public class StartVerificationResponse
    {
        public string TokenId { get; set; }
        public string VerificationId { get; set; }
        public List<AdditionalVerification> AdditionalVerifications { get; set; }
    }
    
    public class VerifyRequest
    {
        public string TokenId { get; set; }
        public string VerificationId { get; set; }
        public string Code { get; set; }
    }

    public class VerifyResponse
    {
        public bool VerificationValid { get; set; }
        public bool TokenValid { get; set; }
        public List<AdditionalVerification> AdditionalVerifications { get; set; }
    }

    public class Token
    {
        public string ClientId { get; set; }
        public string TokenId { get; set; }
        public List<Verification> Verifications { get; set; }
    }

    public class Verification
    {
        public string VerificationId { get; set; }
        public VerificationType VerificationType { get; set; }
    }
    
    [Authorize]
    [ApiController]
    [Route("/api/v1/verification")]
    public class VerificationController : Controller
    {
        private static readonly Dictionary<string, Token> _tokens = new(); 

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

            var tokenId = request.TokenId;
            var verificationId = request.VerificationId;
            var verifications = new List<AdditionalVerification>();

            if (string.IsNullOrWhiteSpace(request.TokenId))
            {
                tokenId = Guid.NewGuid().ToString("N");
                verificationId = Guid.NewGuid().ToString("N");
                var token = new Token()
                {
                    ClientId = clientId,
                    TokenId = tokenId,
                    Verifications = new List<Verification>()
                    {
                        new()
                        {
                            VerificationId = verificationId,
                            VerificationType = request.Type
                        }
                    }
                };

                if (request.Reason == VerificationReason.TwoFaDisable)
                {
                    token.Verifications.Add(new Verification()
                    {
                        VerificationId = Guid.NewGuid().ToString("N"),
                        VerificationType = request.Type == VerificationType.Email ? VerificationType.SMS : VerificationType.Email
                    });
                }
                _tokens[tokenId] = token;
                
                verifications = token.Verifications.Where(t => t.VerificationId != verificationId).Select(t=>new AdditionalVerification()
                {
                    Type = t.VerificationType,
                    VerificationId = t.VerificationId
                }).ToList();
            }
            else
            {
                if( !_tokens.TryGetValue(request.TokenId, out var token))
                {
                    return new Response<StartVerificationResponse>(ApiResponseCodes.InternalServerError);
                }

                verifications = token.Verifications.Where(t => t.VerificationId != request.VerificationId).Select(t=>new AdditionalVerification()
                {
                    Type = t.VerificationType,
                    VerificationId = t.VerificationId
                }).ToList();
            }

            return new Response<StartVerificationResponse>(
                new StartVerificationResponse
                {
                    TokenId = tokenId,
                    VerificationId = verificationId,
                    AdditionalVerifications = verifications
                });
        }
        
        [HttpPost("verify")]
        public async Task<Response<VerifyResponse>> VerifyEmailCodeAsync([FromBody] VerifyRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return new Response<VerifyResponse>(ApiResponseCodes.InvalidCode);
            
            if( !_tokens.TryGetValue(request.TokenId, out var token))
                return new Response<VerifyResponse>(ApiResponseCodes.InvalidCode);

            if (!string.IsNullOrWhiteSpace(request.Code))
                token.Verifications.Remove(
                    token.Verifications.FirstOrDefault(t => t.VerificationId == request.VerificationId));
            
            
            return new Response<VerifyResponse>(new VerifyResponse
            {
                VerificationValid = !string.IsNullOrWhiteSpace(request.Code),
                TokenValid = !token.Verifications.Any(),
                AdditionalVerifications = token.Verifications.Where(t => t.VerificationId != request.VerificationId).Select(t=>new AdditionalVerification()
                {
                    Type = t.VerificationType,
                    VerificationId = t.VerificationId
                }).ToList()
            });
        }
        
    }
}