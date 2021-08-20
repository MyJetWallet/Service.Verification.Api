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
    [Route("/api/v1/withdrawal-verification")]
    public class WithdrawalVerificationController : Controller
    {
        private readonly IWithdrawalVerificationService _withdrawalVerificationService;
        
        public WithdrawalVerificationController(IWithdrawalVerificationService withdrawalVerificationService)
        {
            _withdrawalVerificationService = withdrawalVerificationService;
        }

        [HttpPost("request")]
        public async Task<Response> RequestWithdrawalVerificationCodeAsync([FromBody] SendWithdrawalVerificationRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            var sendRequest = new SendWithdrawalVerificationCodeRequest
            {
                Lang = request.Language,
                ClientId = clientId,
                OperationId = request.OperationId,
                AssetSymbol = request.AssetSymbol,
                Amount = request.Amount,
                DestinationAddress = request.DestinationAddress,
                IpAddress = accessor.HttpContext.GetIp(),
                Brand = this.GetBrandId()
            };
            var response = await _withdrawalVerificationService.SendWithdrawalVerificationCodeAsync(sendRequest);
            return response.IsSuccess 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.UnsuccessfulSend);
        }
        
        [AllowAnonymous]
        [HttpGet("verify")]
        public async Task<ActionResult> VerifyWithdrawalCodeAsync([FromQuery] string withdrawalProcessId, string code, string brand, [FromServices] IHttpContextAccessor accessor)
        {
            var verifyRequest = new VerifyWithdrawalCodeRequest()
            {
                WithdrawalProcessId = withdrawalProcessId,
                Code = code,
                ClientIp = accessor.HttpContext.GetIp(),
                Brand = brand
            };
            var response = await _withdrawalVerificationService.VerifyWithdrawalCodeAsync(verifyRequest);
            return Redirect(response.RedirectLink);
        }
    }
}