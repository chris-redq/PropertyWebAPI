//-----------------------------------------------------------------------
// <copyright file="APIKeySecurityHandler.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

#pragma warning disable 1591
namespace PropertyWebAPI
{
    using System.Net.Http;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    public class APIKeySecurityHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Security.User userObj = Security.Authentication.AuthenticateOld(request);
            if (userObj != null)
            {
                var accountId = userObj.AccountId;
                var principal = new ClaimsPrincipal(new GenericIdentity(accountId, "AccountId"));
                HttpContext.Current.User = principal;
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
#pragma warning restore 1591