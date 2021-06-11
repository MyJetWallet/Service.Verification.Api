using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyJetWallet.Domain;
using MyJetWallet.Sdk.Authorization.Http;

namespace Service.Verification.Api.Controllers
{
    public static class ControllerUtils
    {
        public static JetClientIdentity GetClientIdentity(this ControllerBase controller)
        {
            var id = new JetClientIdentity(controller.GetBrokerId(), controller.GetBrandId(), controller.GetClientId());
            return id;
        }
        
        /// <summary>
        /// Get Ip of request
        /// </summary>
        /// <param name="ctx">Request context</param>
        /// <returns></returns>
        public static string GetIp(this HttpContext ctx)
        {
            return ctx.Request.GetIp();
        }
        
        private static string GetIp(this HttpRequest httpRequest)
        {
            foreach (var ipHeader in IpHeaders)
            {
                if (httpRequest.Headers.ContainsKey(ipHeader))
                    return httpRequest.Headers[ipHeader].ToString();
            }

            return httpRequest?.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        
        private static readonly string[] IpHeaders =
        {
            "CF-Connecting-IP", "X-Forwarded-For"
        };
    }
}