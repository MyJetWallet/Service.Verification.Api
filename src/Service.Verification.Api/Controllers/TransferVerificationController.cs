using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Verification.Api.Controllers.Contracts;
using Service.VerificationCodes.Grpc;
using Service.VerificationCodes.Grpc.Models;
using SimpleTrading.PersonalData.Abstractions.Auth.Consts;

namespace Service.Verification.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/transfer-verification")]
    public class TransferVerificationController : Controller
    {
        private readonly ITransferVerificationService _transferVerificationService;
        
        public TransferVerificationController(ITransferVerificationService transferVerificationService)
        {
            _transferVerificationService = transferVerificationService;
        }

        [HttpPost("request")]
        public async Task<Response> RequestWithdrawalVerificationCodeAsync([FromBody] SendTransferVerificationRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            if(string.IsNullOrWhiteSpace(request.Language))
                return new Response(ApiResponseCodes.LanguageNotSet);
            
            var clientId = this.GetClientIdentity().ClientId;
            if (clientId == SpecialUserIds.EmptyUser.ToString("N"))
                return Contracts.Response.OK();
            
            var sendRequest = new SendTransferVerificationCodeRequest()
            {
                Lang = request.Language,
                ClientId = clientId,
                OperationId = request.OperationId,
                AssetSymbol = request.AssetSymbol,
                Amount = request.Amount,
                DestinationPhone = request.DestinationPhone,
                IpAddress = accessor.HttpContext.GetIp()
            }; 
            var response = await _transferVerificationService.SendTransferVerificationCodeAsync(sendRequest);
            return response.IsSuccess 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.UnsuccessfulSend);
        }
        
        [AllowAnonymous]
        [HttpGet("verify")]
        public async Task<ActionResult> VerifyTransferAsync([FromQuery] string transferProcessId, string code, string brand, [FromServices] IHttpContextAccessor accessor)
        {
            var verifyRequest = new VerifyTransferCodeRequest()
            {
                TransferId = transferProcessId,
                Code = code,
                ClientIp = accessor.HttpContext.GetIp(),
                Brand = brand
            };
            var response = await _transferVerificationService.VerifyTransferCodeAsync(verifyRequest);
            return Redirect(response.RedirectLink);
        }
        
        [AllowAnonymous]
        [HttpPost("verify-code")]
        public async Task<Response> VerifyTransferCodeAsync([FromBody] OperationVerificationRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var verifyRequest = new VerifyTransferCodeRequest()
            {
                TransferId = request.OperationId,
                Code = request.Code,
                ClientIp = accessor.HttpContext.GetIp(),
                Brand = request.Brand
            };
            var response = await _transferVerificationService.VerifyTransferCodeAsync(verifyRequest);
            return response.CodeIsValid 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.InvalidCode);
        }

        
        [AllowAnonymous]
        [HttpPost("verify")]
        public async Task<Response> VerifyTransferCodeWithTokenAsync([FromBody] TokenRequest request, [FromServices] IHttpContextAccessor accessor)
        {
            var response = await _transferVerificationService.VerifyTransferCodeWithTokenAsync(new VerifyTransferCodeWithTokenRequest()
            {
                Token = request.Token,
                ClientIp = accessor.HttpContext.GetIp(),
            });
            return response.CodeIsValid 
                ? Contracts.Response.OK()
                : new Response(ApiResponseCodes.InvalidCode);
        }
    }
}