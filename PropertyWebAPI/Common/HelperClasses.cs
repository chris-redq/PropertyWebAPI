using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Results;

namespace PropertyWebAPI.Common
{
    public class HelperClasses
    {
    }

    public static class HttpResponse
    {
        public static ResponseMessageResult InternalError(HttpRequestMessage Request, string message)
        {
            return new ResponseMessageResult(Request.CreateErrorResponse((HttpStatusCode)500, message));
        }
    }
}