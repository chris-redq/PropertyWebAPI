//-----------------------------------------------------------------------
// <copyright file="Request.cs" company="Redq Technologies, Inc.">
//     Copyright (c) Redq Technologies, Inc. All rights reserved.
// </copyright>
// <author>Raj Sethi</author>
//-----------------------------------------------------------------------

namespace PropertyWebAPI.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using WebDataDB;

    /// <summary>
    ///     This class abstracts all detailed level operations associated with finding, inserting and reading records in the Request table in WebData DB.
    ///     In essence it abstracts semantic level operations for the table in the context of the API 
    /// </summary>
    public static class Request
    {
        /// <summary>
        ///     Inserts a Request object(row) for a given BBL and type of request when data (not stale) is found in WebData DB
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="requestStr">
        ///     Depending on the Request Type a helper class will be provided to convert business level variables into a string (in a specific format)
        ///     such that it is readable by the web scarping service. 
        /// </param>
        /// <param name="requestTypeId"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public static WebDataDB.Request Insert(WebDataEntities webDBEntities, string requestStr, int requestTypeId, string jobId)
        {
            WebDataDB.Request requestObj = new WebDataDB.Request();
            requestObj.JobId = jobId;
            requestObj.Priorty = 5;
            requestObj.RequestTypeId = requestTypeId;
            requestObj.RequestStatusTypeId = (int)RequestStatus.Pending;
            requestObj.DateTimeSubmitted = DateTime.UtcNow;
            requestObj.AttemptNumber = 0;
            requestObj.RequestData = requestStr; 
            
            requestObj = webDBEntities.Requests.Add(requestObj);
            webDBEntities.SaveChanges();

            return requestObj;
        }

        /// <summary>
        ///     Inserts a Request object(row) for a given BBL and type of request when data (not stale) is found in WebData DB
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="requestStr">
        ///     Depending on the Request Type a helper class will be provided to convert business level variables into a string (in a specific format)
        ///     such that it is readable by the web scarping service. 
        /// </param>
        /// <param name="requestTypeId"></param>
        /// <param name="priority"></param>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public static WebDataDB.Request Insert(WebDataEntities webDBEntities, string requestStr, int requestTypeId, int priority, string jobId)
        {
            WebDataDB.Request requestObj = new WebDataDB.Request();
            requestObj.JobId = jobId;
            requestObj.Priorty = priority;
            requestObj.RequestTypeId = requestTypeId;
            requestObj.RequestStatusTypeId = (int)RequestStatus.Pending;
            requestObj.DateTimeSubmitted = DateTime.UtcNow;
            requestObj.AttemptNumber = 0;
            requestObj.RequestData = requestStr;

            requestObj = webDBEntities.Requests.Add(requestObj);
            webDBEntities.SaveChanges();

            return requestObj;
        }
    }
}