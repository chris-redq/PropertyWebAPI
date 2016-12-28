//-----------------------------------------------------------------------
// <copyright file="CasesController.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------
#pragma warning disable 1591
namespace PropertyWebAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Configuration;

    public static class AppSettings
    {
        public const int GEO_CLIENT_APP_ID = 1;
        public const int GEO_CLIENT_APP_KEY = 2;
        public const int PORTAL_API_KEY = 3;
        public const int PORTAL_BASE_URL = 4;
        public const int PORTAL_CALLBACK_API = 5;

        public static string Get(int tokenId)
        {
            switch(tokenId)
            {
                case GEO_CLIENT_APP_ID:
                    return ConfigurationManager.AppSettings["geoClientAppId"];
                case GEO_CLIENT_APP_KEY:
                    return ConfigurationManager.AppSettings["geoClientAppKey"];
                case PORTAL_API_KEY:
                    return ConfigurationManager.AppSettings["portalAPIKey"];
                case PORTAL_BASE_URL:
                    return ConfigurationManager.AppSettings["portalBaseURL"];
                case PORTAL_CALLBACK_API:
                    return ConfigurationManager.AppSettings["portalCallBackAPI"];
                default:
                    return null;
            }
        }
    }
}
#pragma warning restore 1591