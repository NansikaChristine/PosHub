using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PosHubApi.Errors
{
    public class ApiExceptions : Exception
    {
        public ApiExceptions(int statusCode, string message = null, string details = null)
            : base(message)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }

        public int StatusCode { get; set; }
        public string Details { get; set; }

        public override string Message { get; }
    }
}