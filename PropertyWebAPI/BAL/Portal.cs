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
    using WebAPISecurityDB;

    public class CallingSystem
    {
        /// <summary>  
        ///     Method returns address corrections and details based on street number, street address and borough for NYC properties
        /// </summary>  
        public static void PostCallBack(string accountId, CallBack cb, BAL.Results result)
        {
            var userObj = Security.Authentication.GetUser(accountId);

            if (userObj == null)
                return;

            if (cb == null)
                return;

            PostCallBack(cb.URL, cb.API, cb.APIKey, userObj.UserName, result);
        }

        /// <summary>  
        ///     Method returns address corrections and details based on street number, street address and borough for NYC properties
        /// </summary>  
        public static void PostCallBack(string baseURL, string api, string apiKey, string username, BAL.Results result)
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
                    Common.Logs.log().Error(string.Format("{0} API callback failed with status code {1}", username, response.StatusCode));
                }
            }
            catch (Exception e)
            {
                Common.Logs.log().Error(string.Format("{0} API callback failed{1}", username, Common.Logs.FormatException(e)));
            }
        }

        public static CallBack isAnyCallBack(string accountId)
        {
            return Security.Authentication.GetCallBack(accountId, "WEBDATA");
         
        }
    }
}