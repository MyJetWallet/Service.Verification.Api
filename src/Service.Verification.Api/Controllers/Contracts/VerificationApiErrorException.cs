using System;

namespace Service.Verification.Api.Controllers.Contracts
{
    public class VerificationApiErrorException : Exception
    {
        public ApiResponseCodes Code { get; set; }

        public VerificationApiErrorException(string message, ApiResponseCodes code) : base(message)
        {
            Code = code;
        }
    }
}