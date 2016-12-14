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
    ///     In essence it abstracts semantic level operations for the table in the context of the API 
    /// </summary>
    public static class DataRequestLog
    {
        /// <summary>
        ///     Gets a DataRequestLog object(row) from the DataRequestLog table for a given BBL and type of request with Pending status 
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="propertyBBL"></param>
        /// <param name="RequestTypeId"></param>
        /// <param name="requestParameters"></param>
        /// <returns></returns>
        public static WebDataDB.DataRequestLog GetPendingRequest(WebDataEntities webDBEntities, string propertyBBL, int RequestTypeId, string requestParameters)
        {
            return webDBEntities.DataRequestLogs.FirstOrDefault(i => i.RequestStatusTypeId == (int)RequestStatus.Pending
                                                                     && i.RequestTypeId == RequestTypeId
                                                                     && i.BBL == propertyBBL
                                                                     && i.RequestParameters == requestParameters);

        }

        /// <summary>
        ///     Gets a DataRequestLog object(row) from the DataRequestLog table for a given RequestId
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static WebDataDB.DataRequestLog GetFirst(WebDataEntities webDBEntities, long requestId)
        {
            return webDBEntities.DataRequestLogs.FirstOrDefault(i => i.RequestId == requestId);
        }

        /// <summary>
        ///     Gets all DataRequestLog objects(rows) from the DataRequestLog table for a given externalReferenceId
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="externalRefereceId"></param>
        /// <returns></returns>
        public static List<WebDataDB.DataRequestLog> GetAll(WebDataEntities webDBEntities, string externalRefereceId)
        {
            return webDBEntities.DataRequestLogs.Where(i => i.ExternalReferenceId == externalRefereceId).ToList();
        }

        /// <summary>
        ///     Gets all DataRequestLog objects(rows) from the DataRequestLog table for a given RequestId and sets their status to Error
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static List<WebDataDB.DataRequestLog> SetAsError(WebDataEntities webDBEntities, long requestId)
        {
            List<WebDataDB.DataRequestLog> requestDataLogList = webDBEntities.DataRequestLogs.Where(i => i.RequestId == requestId).ToList();
            requestDataLogList.ForEach(a => a.RequestStatusTypeId = (int)RequestStatus.Error);
            webDBEntities.SaveChanges();

            return requestDataLogList;
        }

        /// <summary>
        ///     Gets all DataRequestLog objects(rows) from the DataRequestLog table for a given RequestId and sets their status to Error
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public static List<WebDataDB.DataRequestLog> SetAsSuccess(WebDataEntities webDBEntities, long requestId)
        {
            List<WebDataDB.DataRequestLog> requestDataLogList = webDBEntities.DataRequestLogs.Where(i => i.RequestId == requestId).ToList();
            requestDataLogList.ForEach(a => a.RequestStatusTypeId = (int)RequestStatus.Success);
            webDBEntities.SaveChanges();

            return requestDataLogList;
        }

        /// <summary>
        ///     Inserts a DataRequestLog object(row) for a given BBL and type of request when data (not stale) is found in WebData DB
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="propertyBBL"></param>
        /// <param name="requestTypeId"></param>
        /// <param name="externalReferenceId"></param>
        /// <param name="requestParameters"></param>
        /// <returns></returns>
        public static WebDataDB.DataRequestLog InsertForCacheAccess(WebDataEntities webDBEntities, string propertyBBL, int requestTypeId, 
                                                                    string externalReferenceId, string requestParameters)
        {
            WebDataDB.DataRequestLog dataRequestLogObj = new WebDataDB.DataRequestLog();
            dataRequestLogObj.BBL = propertyBBL;
            dataRequestLogObj.RequestStatusTypeId = (int)RequestStatus.Success;
            dataRequestLogObj.RequestTypeId = requestTypeId;
            dataRequestLogObj.RequestDateTime = DateTime.UtcNow;
            dataRequestLogObj.ExternalReferenceId = externalReferenceId;
            dataRequestLogObj.ServedFromCache = true;
            dataRequestLogObj.WebDataRequestMade = false;
            dataRequestLogObj.RequestParameters = requestParameters;

            dataRequestLogObj = webDBEntities.DataRequestLogs.Add(dataRequestLogObj);
            webDBEntities.SaveChanges();

            return dataRequestLogObj;
        }

        /// <summary>
        ///     Inserts a DataRequestLog object(row) for a given BBL and type of request when data (not stale) is found in WebData DB
        /// </summary>
        /// <param name="propertyBBL"></param>
        /// <param name="requestTypeId"></param>
        /// <param name="externalReferenceId"></param>
        /// <param name="requestParameters"></param>
        /// <returns></returns>
        public static WebDataDB.DataRequestLog InsertForFailure(string propertyBBL, int requestTypeId, string externalReferenceId, string requestParameters)
        {
            WebDataDB.DataRequestLog dataRequestLogObj = new WebDataDB.DataRequestLog();

            using (WebDataEntities webDBEntities = new WebDataEntities())
            {   try
                {
                    dataRequestLogObj.BBL = propertyBBL;
                    dataRequestLogObj.RequestStatusTypeId = (int)RequestStatus.Error;
                    dataRequestLogObj.RequestTypeId = requestTypeId;
                    dataRequestLogObj.RequestDateTime = DateTime.UtcNow;
                    dataRequestLogObj.ExternalReferenceId = externalReferenceId;
                    dataRequestLogObj.ServedFromCache = false;
                    dataRequestLogObj.WebDataRequestMade = false;
                    dataRequestLogObj.RequestParameters = requestParameters;

                    dataRequestLogObj = webDBEntities.DataRequestLogs.Add(dataRequestLogObj);
                    webDBEntities.SaveChanges();
                    return dataRequestLogObj;
                }
                catch(Exception e)
                {   Common.Logs.log().Error(string.Format("Exception encountered request for BBL {0} for Request Type {1} with externalRefId {2}{3}", propertyBBL, 
                                                          ((RequestStatus)requestTypeId).ToString(), externalReferenceId, Common.Utilities.FormatException(e)));
                    return null;
                }
            }
            
        }
        /// <summary>
        ///     Inserts a DataRequestLog object(row) for a given BBL and type of request with Pending status when data is not found or is found but stale in WebData DB
        /// </summary>
        /// <param name="webDBEntities"></param>
        /// <param name="propertyBBL"></param>
        /// <param name="requestTypeId"></param>
        /// <param name="requestId"></param>
        /// <param name="externalReferenceId"></param>
        /// <param name="requestParameters"></param>
        /// <returns></returns>
        public static WebDataDB.DataRequestLog InsertForWebDataRequest(WebDataEntities webDBEntities, string propertyBBL, int requestTypeId, long requestId, 
                                                                       string externalReferenceId, string requestParameters)
        {
            WebDataDB.DataRequestLog dataRequestLogObj = new WebDataDB.DataRequestLog();

            dataRequestLogObj.BBL = propertyBBL;
            dataRequestLogObj.RequestStatusTypeId = (int)RequestStatus.Pending;
            dataRequestLogObj.RequestTypeId = requestTypeId;
            dataRequestLogObj.RequestDateTime = DateTime.UtcNow;
            dataRequestLogObj.ExternalReferenceId = externalReferenceId;
            dataRequestLogObj.ServedFromCache = false;
            dataRequestLogObj.WebDataRequestMade = true;
            dataRequestLogObj.RequestId = requestId;
            dataRequestLogObj.RequestParameters = requestParameters;
            
            dataRequestLogObj = webDBEntities.DataRequestLogs.Add(dataRequestLogObj);
            webDBEntities.SaveChanges();

            return dataRequestLogObj;
        }
    }
}