//-----------------------------------------------------------------------
// <copyright file="Portal.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------
namespace PropertyWebAPI.BAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;
    using System.Web.Http.Description;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using AutoMapper;
    using System.Net.Http.Headers;

    public class Portal
    {
        /// <summary>  
        ///     Helper class for relevant api information to connect and  
        /// </summary>  
        private class ApiDetails
        {
            public string baseURL;
            public string apiKey;
            public string callbackapi;
        }

        private static ApiDetails GetApiDetails()
        {
            ApiDetails apiDetails = new ApiDetails();

            apiDetails.baseURL = AppSettings.Get(AppSettings.PORTAL_BASE_URL);
            apiDetails.apiKey = AppSettings.Get(AppSettings.PORTAL_API_KEY);
            apiDetails.callbackapi = AppSettings.Get(AppSettings.PORTAL_CALLBACK_API);

            return apiDetails;
        }

        /// <summary>  
        ///     Method returns address corrections and details based on street number, street address and borough for NYC properties
        /// </summary>  
        public static void PostCallBack(BAL.Results result)
        {
            ApiDetails apiDetails = GetApiDetails();

            PostCallBack(apiDetails.baseURL, apiDetails.callbackapi, apiDetails.apiKey, result);
        }

        /// <summary>  
        ///     Method returns address corrections and details based on street number, street address and borough for NYC properties
        /// </summary>  
        public static void PostCallBack(string baseURL, string api, string apiKey, BAL.Results result)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseURL);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("apiKey", apiKey);

            try
            {
                HttpResponseMessage response = client.PostAsJsonAsync(api, result).Result;
                if (!response.IsSuccessStatusCode)
                {
                    Common.Logs.log().Error(string.Format("Portal API callback failed with status code", response.StatusCode));
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("Portal API callback failed{0}", Common.Utilities.FormatException(e)));
            }
        }
    }
}