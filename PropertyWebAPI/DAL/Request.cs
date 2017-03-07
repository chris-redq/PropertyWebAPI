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
    using System.Data.Entity;
    using System.Linq;
    using System.Web;
    using WebDataDB;

    /// <summary>
    ///     This class abstracts all detailed level operations associated with finding, inserting and reading records in the Request table in WebData DB.
    ///     In essence it abstracts semantic level operations for the table in the context of the API 
    /// </summary>
    public static class Request
    {
        public const int HIGHPRIORITY = 3;
        public const int MEDIUMPRIORITY = 5;
        public const int STALEDATAPRIORITY = 6;
        public const int BULKDATAPRIORITY = 7;
        public const int LOWPRIORITY = 8;

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


        /// <summary>
        ///     Update Request object with higher priority and Job Id
        /// </summary>
        public static WebDataDB.Request Update(WebDataEntities webDBEntities, long requestId,  int priority, string jobId)
        {

            var requestObj = webDBEntities.Requests.Where(x => x.RequestId == requestId).FirstOrDefault();
            if (requestObj == null)
                return null;

            requestObj.JobId = jobId;
            if (requestObj.Priorty > priority)
                requestObj.Priorty = priority;

            webDBEntities.Requests.Attach(requestObj);
            webDBEntities.Entry(requestObj).State = EntityState.Modified;
            webDBEntities.SaveChanges();

            return requestObj;
        }
    }
}