using System.Text.Json.Serialization;

namespace Service.Verification.Api.Controllers.Contracts
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApiResponseCodes
    {
        OK = 0,
        InternalServerError = 1,
        InvalidCode = 2,
        UnsuccessfulSend = 3,
        PhoneIsNotConfirmed = 4,
    }
}