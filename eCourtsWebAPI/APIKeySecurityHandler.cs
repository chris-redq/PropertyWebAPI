using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using WebAPISecurityDB;

namespace eCourtsWebAPI
{
    public class APIKeySecurityHandler:DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var queryString = request.RequestUri.ParseQueryString();
            var apiKey = queryString["apiKey"];
            if (apiKey == null)
            {   try
                {
                    apiKey = request.Headers.GetValues("apiKey").FirstOrDefault();
                }
                catch(System.InvalidOperationException e)
                {

                }
            }

            if (apiKey != null)
            {   WebAPISecurityEntities SE = new WebAPISecurityEntities();
                var userObj = SE.APIKeyUsers.Where(x => x.APIKey.ToString() == apiKey).FirstOrDefault();
                if (userObj != null)
                {
                    var userName = userObj.UserName;
                    var principal = new ClaimsPrincipal(new GenericIdentity(userName, "APIKey"));
                    HttpContext.Current.User = principal;
                }
            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}