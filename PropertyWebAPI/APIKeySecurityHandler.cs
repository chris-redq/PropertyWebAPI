//-----------------------------------------------------------------------
// <copyright file="APIKeySecurityHandler.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI
{
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

    public class APIKeySecurityHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Check if the api key is in the query string if not check the request header for the api key
            var queryString = request.RequestUri.ParseQueryString();
            var apiKey = queryString["apiKey"];
            if (apiKey == null)
            {
                try
                {
                    apiKey = request.Headers.GetValues("apiKey").FirstOrDefault();
                }
                catch (System.InvalidOperationException e)
                {
                    Common.Logs.log().Error(string.Format("apiKey token not found in HTTP Header\n{0}\n",e.ToString()));
                }
            }

            // Once the api key is found, check if valid api key. If valid key then create a principal with the 
            // username and set the principal in HtppContext
            if (apiKey != null)
            {
                try
                {
                    using (WebAPISecurityEntities securityDBEntities = new WebAPISecurityEntities())
                    {
                        var userObj = securityDBEntities.APIKeyUsers.Where(x => x.APIKey.ToString() == apiKey).FirstOrDefault();
                        if (userObj != null)
                        {
                            var userName = userObj.UserName;
                            var principal = new ClaimsPrincipal(new GenericIdentity(userName, "APIKey"));
                            HttpContext.Current.User = principal;
                        }
                        else
                            Common.Logs.log().Error(String.Format("apiKey {0} not found in DB", apiKey));
                    }
                }
                catch (Exception e)
                {
                    Common.Logs.log().Error(string.Format("Error in acquiring HttpContext.Current.User\n{0}\n", e.ToString()));
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}