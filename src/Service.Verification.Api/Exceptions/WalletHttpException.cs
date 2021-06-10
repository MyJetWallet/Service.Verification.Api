using System;
using System.Net;

namespace Service.Verification.Api.Exceptions
{
    public class MyHttpException: Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public MyHttpException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}