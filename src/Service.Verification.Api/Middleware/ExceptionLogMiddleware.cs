using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Service.Verification.Api.Exceptions;

// ReSharper disable UnusedMember.Global

namespace Service.Verification.Api.Middleware
{
    public class ExceptionLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionLogMiddleware> _logger;

        public ExceptionLogMiddleware(RequestDelegate next, ILogger<ExceptionLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (MyHttpException ex)
            {
                ex.FailActivity();
                _logger.LogInformation(ex,"Receive WalletApiHttpException with status code: {StatusCode}; path: {Path}", ex.StatusCode, context.Request.Path);

                context.Response.StatusCode = (int) ex.StatusCode;
                await context.Response.WriteAsJsonAsync(new {ex.Message});
            }
            catch (Exception ex)
            {
                ex.FailActivity();
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

    }
}