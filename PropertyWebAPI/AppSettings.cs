//-----------------------------------------------------------------------
// <copyright file="CasesController.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

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
        
       
        public static string Get(int tokenId)
        {
            switch(tokenId)
            {
                case GEO_CLIENT_APP_ID:
                    return ConfigurationManager.AppSettings["geoClientAppId"];
                case GEO_CLIENT_APP_KEY:
                    return ConfigurationManager.AppSettings["geoClientAppKey"];
                default:
                    return null;
            }
        }
    }
}