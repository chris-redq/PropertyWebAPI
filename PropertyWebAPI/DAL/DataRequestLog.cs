//-----------------------------------------------------------------------
// <copyright file="DataRequestLog.cs" company="Redq Technologies, Inc.">
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
    ///     This class abstracts all detailed level operations associated with finding, inserting and updating records in the DataRequestLog table in WebData DB.
    ///     In essence it abstracts symantic level operations for the table in the context of the API 
    /// </summary>
    public static class DataRequestLog
    {
        /// <summary>
        ///     Gets a DataRequestLog object(row) from the DataRequestLog table for a given BBL and type of request with Pending status 
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="propertyBBL"></param>
        /// <param name="RequestTypeId"></param>
        /// <returns></returns>
        public static WebDataDB.DataRequestLog GetPendingRequest(WebDataEntities webDBEntities, string propertyBBL, int RequestTypeId)
        {
            return webDBEntities.DataRequestLogs.FirstOrDefault(i => i.RequestStatusTypeId == (int)RequestStatus.Pending
                                                                     && i.RequestTypeId == RequestTypeId
                                                                     && i.BBL == propertyBBL);

        }

        /// <summary>
        ///     Inserts a DataRequestLog object(row) for a given BBL and type of request when data (not stale) is found in WebData DB
        /// </summary>
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="propertyBBL"></param>
        /// <param name="requestTypeId"></param>
        /// <param name="externalRequestId"></param>
        /// <returns></returns>
        public static WebDataDB.DataRequestLog InsertForCacheAccess(WebDataEntities webDBEntities, string propertyBBL, int requestTypeId, string externalRequestId)
        {
            WebDataDB.DataRequestLog dataRequestLogObj = new WebDataDB.DataRequestLog();
            dataRequestLogObj.BBL = propertyBBL;
            dataRequestLogObj.RequestStatusTypeId = (int)RequestStatus.Success;
            dataRequestLogObj.RequestTypeId = requestTypeId;
            dataRequestLogObj.RequestDateTime = DateTime.UtcNow;
            dataRequestLogObj.ExternalRequestId = externalRequestId;
            dataRequestLogObj.ServedFromCache = true;

            dataRequestLogObj = webDBEntities.DataRequestLogs.Add(dataRequestLogObj);
            webDBEntities.SaveChanges();

            return dataRequestLogObj;
        }

        /// <summary>
        ///     Inserts a DataRequestLog object(row) for a given BBL and type of request with Pending status when data is not found or is found but stale in WebData DB
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="propertyBBL"></param>
        /// <param name="requestTypeId"></param>
        /// <param name="requestId"></param>
        /// <param name="externalRequestId"></param>
        /// <returns></returns>
        public static WebDataDB.DataRequestLog InsertForWebDataRequest(WebDataEntities webDBEntities, string propertyBBL, int requestTypeId, long requestId, string externalRequestId)
        {
            WebDataDB.DataRequestLog dataRequestLogObj = new WebDataDB.DataRequestLog();

            dataRequestLogObj.BBL = propertyBBL;
            dataRequestLogObj.RequestStatusTypeId = (int)RequestStatus.Pending;
            dataRequestLogObj.RequestTypeId = requestTypeId;
            dataRequestLogObj.RequestDateTime = DateTime.UtcNow;
            dataRequestLogObj.ExternalRequestId = externalRequestId;
            dataRequestLogObj.ServedFromCache = false;
            dataRequestLogObj.RequestId = requestId; 

            dataRequestLogObj = webDBEntities.DataRequestLogs.Add(dataRequestLogObj);
            webDBEntities.SaveChanges();

            return dataRequestLogObj;
        }
    }
}