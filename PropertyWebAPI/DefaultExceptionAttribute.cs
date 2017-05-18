using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace PropertyWebAPI
{
    public class DefaultExceptionAttribute: ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception is Common.AppException)
            {
                HttpResponseMessage msgResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError);
                msgResponse.ReasonPhrase = actionExecutedContext.Exception.Message;
                actionExecutedContext.Response = msgResponse;
            }
            base.OnException(actionExecutedContext);
        }
    }
}